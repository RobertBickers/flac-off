using FlacOff.Lib.Models;

namespace FlacOff.Lib.Services;

/// <summary>
/// Abstraction for scanning directories and discovering audio files
/// </summary>
public interface IDirectoryScanner
{
    /// <summary>
    /// Scans directory recursively for FLAC and MP3 files
    /// </summary>
    (List<ConversionTask> flacTasks, List<ConversionTask> mp3Tasks) ScanDirectory(
        string inputDirectory,
        string outputDirectory);

    /// <summary>
    /// Returns unique parent folders from the discovered files
    /// </summary>
    List<string> GetDetectedFolders(
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks);
}
