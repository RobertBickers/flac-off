namespace FlacOff.Utilities;

/// <summary>
/// Abstraction for logging to console and file
/// </summary>
public interface ILogger
{
    void LogInfo(string message);
    void LogWarning(string message);
    void LogError(string message);
}
