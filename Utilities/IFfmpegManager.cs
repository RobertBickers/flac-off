namespace FlacOff.Utilities;

/// <summary>
/// Abstraction for managing ffmpeg installation and configuration
/// </summary>
public interface IFfmpegManager
{
    /// <summary>
    /// Checks if ffmpeg is installed and handles installation/updates
    /// </summary>
    /// <returns>True if ffmpeg is ready to use, false otherwise</returns>
    bool CheckAndConfigureFfmpeg();
}
