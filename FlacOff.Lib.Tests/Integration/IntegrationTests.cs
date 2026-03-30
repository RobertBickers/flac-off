using NUnit.Framework;
using Microsoft.Extensions.DependencyInjection;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Services;
using FlacOff.Lib.Config;
using FlacOff.Lib.Tests.TestHelpers;
using System.IO;
using System;
using System.Threading.Tasks;

namespace FlacOff.Lib.Tests.Integration;

public class IntegrationTests
{
    [Test]
    public async Task Integration_ConversionPipeline_WithDryRun_CreatesOutputFilesAndReturnsExpectedCounts()
    {
        using var tmp = new TestTempDirectory();
        var input = Path.Combine(tmp.Path, "music");
        var output = Path.Combine(tmp.Path, "out");
        Directory.CreateDirectory(input);
        Directory.CreateDirectory(output);

        var album = Path.Combine(input, "Artist/Album");
        Directory.CreateDirectory(album);
        File.WriteAllText(Path.Combine(album, "t1.flac"), "");
        File.WriteAllText(Path.Combine(album, "t2.flac"), "");
        File.WriteAllText(Path.Combine(input, "s.mp3"), "");

        Environment.SetEnvironmentVariable("FLACOFF_DRYRUN", "1");

        var services = new ServiceCollection();
        var logger = new Logger(output);
        services.AddSingleton<ILogger>(logger);
        services.AddSingleton<IDirectoryScanner, DirectoryScanner>();
        services.AddSingleton<IConversionWrapper, ConversionWrapper>();

        var config = new AppConfig
        {
            InputDirectory = input,
            OutputDirectory = output,
            UseParallelProcessing = false,
            MaxDegreeOfParallelism = 2,
            Mp3Bitrate = "320k"
        };

        services.AddSingleton(config);
        services.AddSingleton<IConversionService, ConversionService>();

        var sp = services.BuildServiceProvider();

        var scanner = sp.GetRequiredService<IDirectoryScanner>();
        var (flacTasks, mp3Tasks) = scanner.ScanDirectory(input, output);

        var service = sp.GetRequiredService<IConversionService>();
        var result = await service.ConvertAsync(flacTasks, mp3Tasks);

        Assert.AreEqual(3, result.TotalFiles);
        Assert.AreEqual(2, result.ConvertedFlacCount);
        Assert.AreEqual(1, result.CopiedMp3Count);

        // Check files exist in output
        Assert.IsTrue(File.Exists(Path.Combine(output, "Artist/Album/t1.mp3")));
        Assert.IsTrue(File.Exists(Path.Combine(output, "Artist/Album/t2.mp3")));
        Assert.IsTrue(File.Exists(Path.Combine(output, "s.mp3")));

        // Log file exists
        Assert.IsTrue(File.Exists(Path.Combine(output, "conversion.log")));
    }
}