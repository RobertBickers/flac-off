using System;

namespace FlacOff.Config;

public class AppConfig
{
    public required string InputDirectory { get; set; }
    public required string OutputDirectory { get; set; }
    public bool UseParallelProcessing { get; set; } = false;
    public int MaxDegreeOfParallelism { get; set; } = Environment.ProcessorCount;
    public string Mp3Bitrate { get; set; } = "320k";
}
