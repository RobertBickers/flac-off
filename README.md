# FLAC-to-MP3 Directory Converter

A .NET 8 console application that recursively converts FLAC files to 320kbps MP3s while copying existing MP3s as-is. Includes automated ffmpeg dependency management, progress tracking, dual logging, and retry capabilities.

This tool is designed for users who want to convert their music libraries from FLAC to MP3 format while preserving existing MP3 files. It provides a seamless experience with automatic ffmpeg installation, real-time progress updates, and comprehensive logging.

I created this to get around the issue I have where I want to be able to import my high-resolution FLAC library into my Apple Music for use on my iPhone for integration into other existing hardware, like cars, home media systems, and other bits and pieces, I wanted to do this without having to water down my originals at all which are all stored as FLAC files in my home NAS

## Features

- **Automatic Dependency Management**: Checks if ffmpeg is installed, offers updates, or installs via homebrew automatically
- **Recursive Directory Scanning**: Finds all FLAC and MP3 files in nested directories
- **Pre-Conversion Confirmation**: Shows detected folders and requests user confirmation before starting
- **Live Progress Display**: Shows 0-100% progress bar updating at 1% increments
- **Dual Logging**: Console output + log file in output directory with timestamps
- **Parallel Processing**: Optional configurable multi-threaded conversion
- **Retry Mechanism**: Failed files can be retried within the same run
- **Configurable Settings**: JSON config for input/output directories, parallel settings, and MP3 bitrate

## Output Structure

The output directory mirrors the input directory structure:
```
Input:  /music/
  └── Rock/
      └── Artist Album/
          ├── track1.flac
          ├── track2.flac
          └── booklet.mp3

Output: /output/
  └── Rock/
      └── Artist Album/
          ├── track1.mp3
          ├── track2.mp3
          └── booklet.mp3
```

## Requirements

- .NET 8 SDK or Runtime
- macOS (currently homebrew-dependent; Windows/Linux support planned if there's interest)

## Installation

1. Clone or download this repository
2. Build the project:
   ```bash
   dotnet build -c Release
   ```

## Configuration

Edit `appsettings.json` in the executable directory:

```json
{
  "InputDirectory": "/path/to/your/music/library",
  "OutputDirectory": "/path/to/output/directory",
  "UseParallelProcessing": false,
  "MaxDegreeOfParallelism": 4,
  "Mp3Bitrate": "320k"
}
```

### Configuration Options

| Option | Type | Description | Default |
|--------|------|-------------|---------|
| `InputDirectory` | string | Path to directory containing FLAC/MP3 files | (required) |
| `OutputDirectory` | string | Path where converted files will be saved | (required) |
| `UseParallelProcessing` | bool | Enable multi-threaded conversion | false |
| `MaxDegreeOfParallelism` | int | Max concurrent conversions | CPU core count |
| `Mp3Bitrate` | string | FFmpeg bitrate specification | "320k" |

## Usage

1. Update `appsettings.json` with your directories
2. Run the application:
   ```bash
   ./flac-off
   # or with dotnet
   dotnet run
   ```
3. The application will:
   - Check/install/update ffmpeg automatically
   - Scan your input directory
   - Display detected folders
   - Ask for confirmation
   - Convert files with live progress display
   - Show summary and offer retry for failed files


## Logging

A `conversion.log` file is created in the output directory with timestamped entries for all operations, warnings, and errors. Console output mirrors the log file.

## Architecture

### Core Components

- **FfmpegManager** (`Utilities/FfmpegManager.cs`): Handles ffmpeg installation/updates on macOS
- **ConfigLoader** (`Config/ConfigLoader.cs`): Loads and validates configuration from JSON
- **DirectoryScanner** (`Services/DirectoryScanner.cs`): Recursively finds FLAC/MP3 files
- **ProgressTracker** (`Models/ProgressTracker.cs`): Tracks progress with 1% increment boundaries
- **FfmpegWrapper** (`Services/ConversionWrapper.cs`): Executes ffmpeg CLI and file copying
- **ConversionService** (`Services/ConversionService.cs`): Orchestrates sequential/parallel conversion
- **Logger** (`Utilities/Logger.cs`): Dual console/file logging with timestamps

### Application Flow

1. FFmpeg dependency check/installation
2. Configuration loading and validation
3. Directory scanning for FLAC/MP3 files
4. Display detected folders to user
5. User confirmation prompt
6. Sequential or parallel conversion with progress display
7. Summary display and retry option for failures

## Error Handling

- **ffmpeg Missing**: Automatically installs via homebrew or shows helpful error if homebrew not found
- **Invalid Config**: Validates required fields and directory access
- **Conversion Failures**: Logged per file with error details; users can retry
- **Permission Errors**: Gracefully skips inaccessible directories and logs warnings

## Build & Distribution

Build for release:
```bash
dotnet build -c Release
```

This creates a self-contained executable in `bin/Release/net8.0/flac-off`

## Troubleshooting

### ffmpeg installation fails
- Ensure Homebrew is installed: `/bin/bash -c "$(curl -fsSL https://raw.githubusercontent.com/Homebrew/install/HEAD/install.sh)"`
- Run manually: `brew install ffmpeg`

### Permission denied on directories
- Ensure you have read access to input directory and write access to output directory
- Check file permissions: `chmod u+rwx <directory>`

### Conversion crashes mid-process
- Check disk space on output drive
- Verify FLAC files aren't corrupted: `ffmpeg -v error -i file.flac -f null -`
- Check log file for specific file causing issues, then retry

## License

TODO: Fill this in

## Support

For issues or feature requests, please [[create an issue]](https://github.com/RobertBickers/flac-off/issues)
