namespace FlacOff.Lib.Utilities;

/// <summary>
/// Manages dual logging to console and file
/// </summary>
public class Logger : ILogger
{
    private readonly string? _logFilePath;
    private readonly object _lockObject = new();

    public Logger(string? outputDirectory = null)
    {
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            _logFilePath = Path.Combine(outputDirectory, "conversion.log");
            // Ensure directory exists
            Directory.CreateDirectory(outputDirectory);
        }
    }

    public void LogInfo(string message) => Log("INFO", message);

    public void LogWarning(string message) => Log("WARN", message);

    public void LogError(string message) => Log("ERROR", message);

    private void Log(string level, string message)
    {
        lock (_lockObject)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var logEntry = $"[{timestamp}] [{level}] {message}";

            // Console output
            Console.WriteLine(message);

            // File output if configured
            if (!string.IsNullOrWhiteSpace(_logFilePath))
            {
                try
                {
                    File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Failed to write to log file: {ex.Message}");
                }
            }
        }
    }
}
