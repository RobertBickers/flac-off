using System;
using System.IO;
using System.Text.Json;
using FlacOff.Utilities;

namespace FlacOff.Config;

/// <summary>
/// Loads and validates application configuration from appsettings.json
/// </summary>
public class ConfigLoader(ILogger logger) : IConfigLoader
{
    /// <summary>
    /// Loads configuration from appsettings.json in the executable directory
    /// </summary>
    public AppConfig LoadConfig()
    {
        try
        {
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var exeDirectory = Path.GetDirectoryName(exePath)
                ?? throw new InvalidOperationException("Could not determine executable directory");
            
            var configPath = Path.Combine(exeDirectory, "appsettings.json");

            // If not found next to the executable (common during dotnet run), try current working directory
            if (!File.Exists(configPath))
            {
                var cwdPath = Path.Combine(Environment.CurrentDirectory, "appsettings.json");
                if (File.Exists(cwdPath))
                {
                    logger.LogInfo($"Configuration file found in working directory: {cwdPath}");
                    configPath = cwdPath;
                }
            }

            if (!File.Exists(configPath))
            {
                throw new FileNotFoundException($"Configuration file not found at {configPath}");
            }

            var json = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<AppConfig>(json)
                ?? throw new InvalidOperationException("Configuration deserialized to null");

            // Expand user home (~) shorthand in configured paths
            config.InputDirectory = ExpandPath(config.InputDirectory);
            config.OutputDirectory = ExpandPath(config.OutputDirectory);

            ValidateConfig(config);

            logger.LogInfo($"Configuration loaded successfully");
            logger.LogInfo($"  Input Directory: {config.InputDirectory}");
            logger.LogInfo($"  Output Directory: {config.OutputDirectory}");
            logger.LogInfo($"  Parallel Processing: {config.UseParallelProcessing}");
            logger.LogInfo($"  MP3 Bitrate: {config.Mp3Bitrate}");

            return config;
        }
        catch (Exception ex)
        {
            logger.LogError($"Failed to load configuration: {ex.Message}");
            throw;
        }
    }

    private string ExpandPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (path.StartsWith("~/") || path == "~")
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (path == "~") return home;
            return Path.Combine(home, path.Substring(2));
        }

        return path;
    }

    private void ValidateConfig(AppConfig config)
    {
        if (string.IsNullOrWhiteSpace(config.InputDirectory))
        {
            throw new InvalidOperationException("InputDirectory is required in configuration");
        }

        if (string.IsNullOrWhiteSpace(config.OutputDirectory))
        {
            throw new InvalidOperationException("OutputDirectory is required in configuration");
        }

        if (!Directory.Exists(config.InputDirectory))
        {
            throw new InvalidOperationException($"Input directory does not exist: {config.InputDirectory}");
        }

        if (config.MaxDegreeOfParallelism < 1)
        {
            throw new InvalidOperationException("MaxDegreeOfParallelism must be at least 1");
        }
    }
}
