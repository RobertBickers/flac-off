using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FlacOff.Config;
using FlacOff.Models;
using FlacOff.Utilities;

namespace FlacOff.Services;

public class ConversionService(IConversionWrapper conversionWrapper, ILogger logger, AppConfig config)
    : IConversionService
{
    public async Task<ConversionResult> ConvertAsync(
        List<ConversionTask> flacTasks,
        List<ConversionTask> mp3Tasks)
    {
        var result = new ConversionResult
        {
            StartTime = DateTime.Now,
            TotalFiles = flacTasks.Count + mp3Tasks.Count
        };

        var allTasks = flacTasks.Concat(mp3Tasks).ToList();

        if (allTasks.Count == 0)
        {
            logger.LogWarning("No files to convert");
            result.EndTime = DateTime.Now;
            return result;
        }

        var progressTracker = new ProgressTracker(allTasks.Count);

        if (config.UseParallelProcessing)
        {
            await ProcessParallelAsync(allTasks, progressTracker, result);
        }
        else
        {
            ProcessSequential(allTasks, progressTracker, result);
        }

        result.EndTime = DateTime.Now;
        return result;
    }

    private void ProcessSequential(
        List<ConversionTask> tasks,
        ProgressTracker progressTracker,
        ConversionResult result)
    {
        foreach (var task in tasks)
        {
            ProcessTask(task, result);

            if (progressTracker.ReportProgress(out var percentage))
            {
                var completedCount = progressTracker.CurrentFileIndex;
                var barDisplay = ProgressTracker.FormatProgressBar(percentage, completedCount, progressTracker.TotalFiles);
                Console.WriteLine($"Converting {barDisplay}");
            }
        }
    }

    private async Task ProcessParallelAsync(
        List<ConversionTask> tasks,
        ProgressTracker progressTracker,
        ConversionResult result)
    {
        var semaphore = new SemaphoreSlim(config.MaxDegreeOfParallelism);
        var conversionTasks = new List<Task>();

        foreach (var task in tasks)
        {
            await semaphore.WaitAsync();

            var conversionTask = Task.Run(() =>
            {
                try
                {
                    ProcessTask(task, result);

                    if (progressTracker.ReportProgress(out var percentage))
                    {
                        var completedCount = progressTracker.CurrentFileIndex;
                        var barDisplay = ProgressTracker.FormatProgressBar(percentage, completedCount, progressTracker.TotalFiles);
                        Console.WriteLine($"Converting {barDisplay}");
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            });

            conversionTasks.Add(conversionTask);
        }

        await Task.WhenAll(conversionTasks);
    }

    private void ProcessTask(ConversionTask task, ConversionResult result)
    {
        try
        {
            task.Status = ConversionStatus.InProgress;

            var success = task.FileType == FileType.Flac
                ? conversionWrapper.ConvertFlacToMp3(task.SourceFilePath, task.DestinationFilePath, config.Mp3Bitrate)
                : conversionWrapper.CopyMp3(task.SourceFilePath, task.DestinationFilePath);

            if (success)
            {
                task.Status = ConversionStatus.Completed;
                task.CompletedAt = DateTime.Now;

                if (task.FileType == FileType.Flac)
                {
                    result.ConvertedFlacCount++;
                }
                else
                {
                    result.CopiedMp3Count++;
                }
            }
            else
            {
                task.Status = ConversionStatus.Failed;
                task.ErrorMessage = "Conversion/copy failed";
                result.FailedCount++;
                result.FailedTasks.Add(task);
            }
        }
        catch (Exception ex)
        {
            task.Status = ConversionStatus.Failed;
            task.ErrorMessage = ex.Message;
            result.FailedCount++;
            result.FailedTasks.Add(task);
            logger.LogError($"Exception processing {task.SourceFilePath}: {ex.Message}");
        }
    }
}
