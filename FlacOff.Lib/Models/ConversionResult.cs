namespace FlacOff.Lib.Models;

public class ConversionResult
{
    public int TotalFiles { get; set; }
    public int ConvertedFlacCount { get; set; }
    public int CopiedMp3Count { get; set; }
    public int FailedCount { get; set; }
    public List<ConversionTask> FailedTasks { get; set; } = [];
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }

    private TimeSpan Duration => EndTime - StartTime;

    public string GetSummary()
    {
        return $"Conversion complete: {ConvertedFlacCount} FLAC converted, {CopiedMp3Count} MP3 copied, {FailedCount} failed. Duration: {Duration.TotalSeconds:F1}s";
    }
}
