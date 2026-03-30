using System.Diagnostics;
using FlacOff.Lib.Utilities;

namespace FlacOff.Lib.Services;

public class ConversionWrapper(ILogger logger) : IConversionWrapper
{
    public bool ConvertFlacToMp3(string sourceFile, string destinationFile, string bitrate)
    {
        var dryRun = Environment.GetEnvironmentVariable("FLACOFF_DRYRUN") == "1";

        try
        {
            // Ensure destination directory exists
            var destinationDir = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            if (dryRun)
            {
                logger.LogInfo($"[dry-run] Would convert {sourceFile} -> {destinationFile} @ {bitrate}");
                // Create an empty file to simulate an output
                File.WriteAllText(destinationFile, string.Empty);
                return true;
            }

            var process = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-i \"{sourceFile}\" -b:a {bitrate} -y \"{destinationFile}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var proc = Process.Start(process))
            {
                if (proc == null)
                {
                    logger.LogError($"Failed to start ffmpeg for: {sourceFile}");
                    return false;
                }

                proc.WaitForExit();

                if (proc.ExitCode != 0)
                {
                    var error = proc.StandardError.ReadToEnd();
                    logger.LogError($"Conversion failed for {Path.GetFileName(sourceFile)}: {error}");
                    return false;
                }

                return true;
            }
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception converting {sourceFile}: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Copies an MP3 file to the destination
    /// </summary>
    public bool CopyMp3(string sourceFile, string destinationFile)
    {
        var dryRun = Environment.GetEnvironmentVariable("FLACOFF_DRYRUN") == "1";

        try
        {
            // Ensure destination directory exists
            var destinationDir = Path.GetDirectoryName(destinationFile);
            if (!string.IsNullOrEmpty(destinationDir) && !Directory.Exists(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            if (dryRun)
            {
                logger.LogInfo($"[dry-run] Would copy {sourceFile} -> {destinationFile}");
                File.WriteAllText(destinationFile, string.Empty);
                return true;
            }

            File.Copy(sourceFile, destinationFile, overwrite: true);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError($"Exception copying {sourceFile}: {ex.Message}");
            return false;
        }
    }
}