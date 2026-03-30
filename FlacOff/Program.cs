using System;
using System.IO;
using FlacOff.Lib.Config;
using FlacOff.Lib.Models;
using FlacOff.Lib.Services;
using FlacOff.Lib.Utilities;
using Microsoft.Extensions.DependencyInjection;

ILogger consoleLogger = new Logger();
var logger = consoleLogger;

try
{
    var configLoader = new ConfigLoader(logger);
    var config = configLoader.LoadConfig();

    var services = new ServiceCollection();
    
    services.AddSingleton(config);
    
    var outputLogger = new Logger(config.OutputDirectory);
    services.AddSingleton<ILogger>(outputLogger);
    
    services.AddSingleton<IFfmpegManager>(sp => new FfmpegManager(sp.GetRequiredService<ILogger>()));
    services.AddSingleton<IConfigLoader>(sp => new ConfigLoader(sp.GetRequiredService<ILogger>()));
    services.AddSingleton<IDirectoryScanner>(sp => new DirectoryScanner(sp.GetRequiredService<ILogger>()));
    services.AddSingleton<IConversionWrapper>(sp => new ConversionWrapper(sp.GetRequiredService<ILogger>()));
    services.AddSingleton<IConversionService>(sp => new ConversionService(
        sp.GetRequiredService<IConversionWrapper>(),
        sp.GetRequiredService<ILogger>(),
        sp.GetRequiredService<AppConfig>()
    ));

    var serviceProvider = services.BuildServiceProvider();
    logger = serviceProvider.GetRequiredService<ILogger>();
    logger.LogInfo("╔════════════════════════════════════════╗");
    logger.LogInfo("║   FLAC-to-MP3 Directory Converter      ║");
    logger.LogInfo("║            .NET 10 Console             ║");
    logger.LogInfo("╚════════════════════════════════════════╝");
    logger.LogInfo("");

    var nonInteractive = Environment.GetEnvironmentVariable("FLACOFF_NONINTERACTIVE") == "1";

    logger.LogInfo("Step 1 of 6: Checking ffmpeg installation...");
    var ffmpegManager = serviceProvider.GetRequiredService<IFfmpegManager>();
    if (!ffmpegManager.CheckAndConfigureFfmpeg())
    {
        logger.LogError("ffmpeg is required to continue. Please install it and try again.");
        Environment.Exit(1);
    }
    logger.LogInfo("✓ ffmpeg ready");
    logger.LogInfo("");

    logger.LogInfo("Step 2 of 6: Configuration loaded");
    logger.LogInfo("✓ Configuration loaded");
    logger.LogInfo("");

    logger.LogInfo("Step 3 of 6: Scanning directory for files...");
    var directoryScanner = serviceProvider.GetRequiredService<IDirectoryScanner>();
    var (flacTasks, mp3Tasks) = directoryScanner.ScanDirectory(config.InputDirectory, config.OutputDirectory);
    var totalFiles = flacTasks.Count + mp3Tasks.Count;
    logger.LogInfo($"✓ Found {flacTasks.Count} FLAC files and {mp3Tasks.Count} MP3 files");
    logger.LogInfo("");

    logger.LogInfo("Step 4 of 6: Detected folders in input directory:");
    var detectedFolders = directoryScanner.GetDetectedFolders(flacTasks, mp3Tasks);
    if (detectedFolders.Count == 0)
    {
        logger.LogWarning("  (No folders detected - files may be in root or no audio files found)");
    }
    else
    {
        foreach (var folder in detectedFolders)
        {
            logger.LogInfo($"  • {folder}");
        }
    }
    logger.LogInfo("");

    logger.LogInfo("Step 5 of 6: Ready to convert");
    if (!nonInteractive)
    {
        Console.Write($"\nProceed with conversion of {totalFiles} files? (Y/N): ");
    }

    var response = nonInteractive
        ? "Y"
        : Console.ReadLine()?.Trim().ToUpperInvariant();

    if (response != "Y" && response != "YES")
    {
        logger.LogInfo("Conversion cancelled by user");
        Environment.Exit(0);
    }

    logger.LogInfo("");
    logger.LogInfo("Step 6 of 6: Starting conversion...");
    logger.LogInfo("");

    var conversionService = serviceProvider.GetRequiredService<IConversionService>();
    var result = await conversionService.ConvertAsync(flacTasks, mp3Tasks);

    logger.LogInfo("");
    logger.LogInfo("═══════════════════════════════════════");
    logger.LogInfo(result.GetSummary());
    logger.LogInfo("═══════════════════════════════════════");

    if (result.FailedCount > 0)
    {
        logger.LogWarning($"\n{result.FailedCount} file(s) failed to convert/copy:");
        foreach (var failedTask in result.FailedTasks)
        {
            logger.LogError($"  • {Path.GetFileName(failedTask.SourceFilePath)}: {failedTask.ErrorMessage}");
        }

        if (!nonInteractive)
        {
            Console.Write("\nWould you like to retry the failed files? (Y/N): ");
        }

        var retryResponse = nonInteractive
            ? "N"
            : Console.ReadLine()?.Trim().ToUpperInvariant();

        if (retryResponse == "Y" || retryResponse == "YES")
        {
            logger.LogInfo("\nRetrying failed files...");
            logger.LogInfo("");

            foreach (var failedTask in result.FailedTasks)
            {
                failedTask.Status = ConversionStatus.Pending;
                failedTask.ErrorMessage = null;
                failedTask.CompletedAt = null;
            }

            var retryResult = await conversionService.ConvertAsync(result.FailedTasks, []);

            logger.LogInfo("");
            logger.LogInfo("═══════════════════════════════════════");
            logger.LogInfo($"Retry complete: {retryResult.ConvertedFlacCount + retryResult.CopiedMp3Count} succeeded, {retryResult.FailedCount} still failed");
            logger.LogInfo("═══════════════════════════════════════");
        }
    }

    logger.LogInfo("\nConversion process complete. Log file saved to: " + Path.Combine(config.OutputDirectory, "conversion.log"));
}
catch (Exception ex)
{
    logger.LogError($"Fatal error: {ex.Message}");
    logger.LogError(ex.StackTrace ?? "");
    Environment.Exit(1);
}
