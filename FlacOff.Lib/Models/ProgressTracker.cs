namespace FlacOff.Lib.Models;

/// <summary>
/// Tracks conversion progress and calculates when to display progress updates at 1% increments
/// </summary>
public class ProgressTracker(int totalFiles)
{
    private int _lastProgressPercentage = -1;

    /// <summary>
    /// Reports progress for the current file and returns whether to display progress update
    /// Updates are shown at each 1% increment boundary
    /// </summary>
    public bool ReportProgress(out int currentPercentage)
    {
        CurrentFileIndex++;
        
        currentPercentage = totalFiles > 0 
            ? (CurrentFileIndex * 100) / totalFiles 
            : 100;

        // Only report if we've crossed a 1% boundary
        if (currentPercentage > _lastProgressPercentage)
        {
            _lastProgressPercentage = currentPercentage;
            return true;
        }

        return false;
    }

    public static string FormatProgressBar(int percentage, int completed, int total)
    {
        const int barWidth = 30;
        var filledWidth = (percentage * barWidth) / 100;
        var emptyWidth = barWidth - filledWidth;

        var bar = new string('█', filledWidth) + new string('░', emptyWidth);
        return $"[{bar}] {percentage}% ({completed} of {total} files)";
    }

    public int CurrentFileIndex { get; private set; } = 0;

    public int TotalFiles => totalFiles;
}
