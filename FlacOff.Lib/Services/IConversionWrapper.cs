namespace FlacOff.Lib.Services;

/// <summary>
/// Abstraction for ffmpeg CLI operations
/// </summary>
public interface IConversionWrapper
{
    /// <summary>
    /// Converts FLAC file to MP3
    /// </summary>
    bool ConvertFlacToMp3(string sourceFile, string destinationFile, string bitrate);

    /// <summary>
    /// Copies MP3 file to destination
    /// </summary>
    bool CopyMp3(string sourceFile, string destinationFile);
}
