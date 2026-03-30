using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlacOff.Models;
using FlacOff.Utilities;

namespace FlacOff.Services;

public class DirectoryScanner(ILogger logger) : IDirectoryScanner
{
    public (List<ConversionTask> flacTasks, List<ConversionTask> mp3Tasks) ScanDirectory(
        string inputDirectory,
        string outputDirectory)
    {
        var flacTasks = new List<ConversionTask>();
        var mp3Tasks = new List<ConversionTask>();

        try
        {
            ScanDirectoryRecursive(
                inputDirectory,
                outputDirectory,
                inputDirectory,
                flacTasks,
                mp3Tasks);

            logger.LogInfo($"Found {flacTasks.Count} FLAC files and {mp3Tasks.Count} MP3 files");

            return (flacTasks, mp3Tasks);
        }
        catch (Exception ex)
        {
            logger.LogError($"Error scanning directory: {ex.Message}");
            throw;
        }
    }

    public List<string> GetDetectedFolders(
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks)
    {
        var folders = new HashSet<string>();

        foreach (var task in flacTasks.Concat(mp3Tasks))
        {
            var relativePath = Path.GetDirectoryName(task.SourceFilePath);
            if (!string.IsNullOrEmpty(relativePath))
            {
                folders.Add(relativePath);
            }
        }

        return folders.OrderBy(f => f).ToList();
    }

    private void ScanDirectoryRecursive(
        string currentDirectory,
        string outputDirectory,
        string inputDirectoryRoot,
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks)
    {
        try
        {
            foreach (var flacFile in Directory.GetFiles(currentDirectory, "*.flac"))
            {
                var relativePath = Path.GetRelativePath(inputDirectoryRoot, flacFile);
                var outputPath = Path.Combine(outputDirectory, Path.ChangeExtension(relativePath, ".mp3"));

                flacTasks.Add(new ConversionTask
                {
                    SourceFilePath = flacFile,
                    DestinationFilePath = outputPath,
                    FileType = FileType.Flac
                });
            }

            foreach (var mp3File in Directory.GetFiles(currentDirectory, "*.mp3"))
            {
                var relativePath = Path.GetRelativePath(inputDirectoryRoot, mp3File);
                var outputPath = Path.Combine(outputDirectory, relativePath);

                mp3Tasks.Add(new ConversionTask
                {
                    SourceFilePath = mp3File,
                    DestinationFilePath = outputPath,
                    FileType = FileType.Mp3
                });
            }

            foreach (var subdirectory in Directory.GetDirectories(currentDirectory))
            {
                try
                {
                    ScanDirectoryRecursive(
                        subdirectory,
                        outputDirectory,
                        inputDirectoryRoot,
                        flacTasks,
                        mp3Tasks);
                }
                catch (UnauthorizedAccessException)
                {
                    logger.LogWarning($"Access denied to directory: {subdirectory}");
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogWarning($"Access denied to directory: {currentDirectory}");
        }
    }
}
