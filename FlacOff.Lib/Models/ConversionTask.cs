namespace FlacOff.Lib.Models;

/// <summary>
/// Represents a single file conversion or copy task
/// </summary>
public record ConversionTask
{
    public required string SourceFilePath { get; set; }
    public required string DestinationFilePath { get; set; }
    public required FileType FileType { get; set; }
    public ConversionStatus Status { get; set; } = ConversionStatus.Pending;
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}