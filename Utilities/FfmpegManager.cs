using System;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace FlacOff.Utilities;

public class FfmpegManager(ILogger logger) : IFfmpegManager
{
    public bool CheckAndConfigureFfmpeg()
    {
        logger.LogInfo("Checking ffmpeg installation...");

        var dryRun = Environment.GetEnvironmentVariable("FLACOFF_DRYRUN") == "1";
        if (dryRun)
        {
            logger.LogInfo("Dry-run mode: simulating ffmpeg availability");
            return true;
        }

        var ffmpegVersion = GetFfmpegVersion();
        var nonInteractive = Environment.GetEnvironmentVariable("FLACOFF_NONINTERACTIVE") == "1";

        if (ffmpegVersion != null)
        {
            logger.LogInfo($"ffmpeg {ffmpegVersion} detected");

            if (!nonInteractive && PromptUserYesNo("Would you like to update ffmpeg? (Y/N): "))
            {
                return UpdateFfmpeg();
            }

            return true;
        }

        logger.LogInfo("ffmpeg not found. Attempting installation via homebrew...");

        if (nonInteractive)
        {
            logger.LogWarning("Non-interactive run: skipping automatic installation of ffmpeg");
            return false;
        }

        if (!IsHomebrewInstalled())
        {
            logger.LogError("Homebrew is not installed. Cannot install ffmpeg automatically.");
            logger.LogError("Please install Homebrew from: https://brew.sh");
            return false;
        }

        return InstallFfmpeg();
    }

    private string? GetFfmpegVersion()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(process))
            {
                if (proc == null)
                    return null;

                proc.WaitForExit(5000);

                if (proc.ExitCode != 0)
                    return null;

                var output = proc.StandardOutput.ReadToEnd();
                var match = Regex.Match(output, @"ffmpeg version (\S+)");
                return match.Success ? match.Groups[1].Value : null;
            }
        }
        catch
        {
            return null;
        }
    }

    private bool IsHomebrewInstalled()
    {
        try
        {
            var process = new ProcessStartInfo
            {
                FileName = "brew",
                Arguments = "--version",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(process))
            {
                if (proc == null)
                    return false;

                proc.WaitForExit(5000);
                return proc.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }

    private bool InstallFfmpeg()
    {
        try
        {
            logger.LogInfo("Installing ffmpeg via homebrew...");
            
            var process = new ProcessStartInfo
            {
                FileName = "brew",
                Arguments = "install ffmpeg",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var proc = Process.Start(process))
            {
                if (proc == null)
                {
                    logger.LogError("Failed to start brew installation process");
                    return false;
                }

                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    logger.LogError($"ffmpeg installation failed: {error}");
                    return false;
                }

                logger.LogInfo("ffmpeg installed successfully");

                var version = GetFfmpegVersion();
                if (version != null)
                {
                    logger.LogInfo($"Verified ffmpeg {version} is available");
                    return true;
                }

                logger.LogWarning("Installation completed but could not verify ffmpeg version");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception during ffmpeg installation: {ex.Message}");
            return false;
        }
    }

    private bool UpdateFfmpeg()
    {
        try
        {
            logger.LogInfo("Updating ffmpeg via homebrew...");

            var process = new ProcessStartInfo
            {
                FileName = "brew",
                Arguments = "upgrade ffmpeg",
                UseShellExecute = false,
                CreateNoWindow = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using (var proc = Process.Start(process))
            {
                if (proc == null)
                {
                    logger.LogError("Failed to start brew upgrade process");
                    return false;
                }

                var output = proc.StandardOutput.ReadToEnd();
                var error = proc.StandardError.ReadToEnd();
                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    logger.LogWarning($"ffmpeg upgrade warning: {error}");
                }

                var version = GetFfmpegVersion();
                if (version != null)
                {
                    logger.LogInfo($"Updated to ffmpeg {version}");
                    return true;
                }

                logger.LogWarning("Update completed but could not verify ffmpeg version");
                return false;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception during ffmpeg update: {ex.Message}");
            return false;
        }
    }

    private static bool PromptUserYesNo(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            var response = Console.ReadLine()?.Trim().ToUpperInvariant();

            if (response == "Y" || response == "YES")
                return true;
            if (response == "N" || response == "NO")
                return false;

            Console.WriteLine("Please enter 'Y' for yes or 'N' for no");
        }
    }
}
