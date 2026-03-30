using NUnit.Framework;
using Moq;
using FlacOff.Lib.Services;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Config;
using FlacOff.Lib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace FlacOff.Lib.Tests;

public class ConversionServiceTests
{
    private Mock<IConversionWrapper> _wrapperMock = null!;
    private Mock<ILogger> _loggerMock = null!;
    private AppConfig _config = null!;

    [SetUp]
    public void Setup()
    {
        _wrapperMock = new Mock<IConversionWrapper>();
        _loggerMock = new Mock<ILogger>();
        _config = new AppConfig
        {
            InputDirectory = ".",
            OutputDirectory = ".",
            UseParallelProcessing = false,
            MaxDegreeOfParallelism = 2,
            Mp3Bitrate = "320k"
        };
    }

    [Test]
    public async Task ConversionService_ConvertAsync_NoTasks_ReturnsEmptyResultAndLogsWarning()
    {
        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync([], []);

        Assert.AreEqual(0, result.TotalFiles);
        _loggerMock.Verify(l => l.LogWarning(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ConversionService_ConvertAsync_AllTasksSucceed_UpdatesCounts()
    {
        var tasks = new List<ConversionTask>
        {
            new() { SourceFilePath = "a.flac", DestinationFilePath = "out/a.mp3", FileType = FileType.Flac },
            new() { SourceFilePath = "b.flac", DestinationFilePath = "out/b.mp3", FileType = FileType.Flac },
            new() { SourceFilePath = "c.mp3", DestinationFilePath = "out/c.mp3", FileType = FileType.Mp3 }
        };

        _wrapperMock.Setup(w => w.ConvertFlacToMp3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _wrapperMock.Setup(w => w.CopyMp3(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync(tasks.Where(t => t.FileType == FileType.Flac).ToList(), tasks.Where(t => t.FileType == FileType.Mp3).ToList());

        Assert.AreEqual(3, result.TotalFiles);
        Assert.AreEqual(2, result.ConvertedFlacCount);
        Assert.AreEqual(1, result.CopiedMp3Count);
        Assert.AreEqual(0, result.FailedCount);
    }

    [Test]
    public async Task ConversionService_ConvertAsync_TaskFailure_RecordsFailedTask()
    {
        var tasks = new List<ConversionTask>
        {
            new() { SourceFilePath = "a.flac", DestinationFilePath = "out/a.mp3", FileType = FileType.Flac }
        };

        _wrapperMock.Setup(w => w.ConvertFlacToMp3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync(tasks, []);

        Assert.AreEqual(1, result.TotalFiles);
        Assert.AreEqual(0, result.ConvertedFlacCount);
        Assert.AreEqual(1, result.FailedCount);
        Assert.AreEqual(1, result.FailedTasks.Count);
        Assert.AreEqual(ConversionStatus.Failed, result.FailedTasks[0].Status);
        Assert.IsNotNull(result.FailedTasks[0].ErrorMessage);
    }

    [Test]
    public async Task ConversionService_ProcessTask_Exception_IsCaughtAndLogged()
    {
        var tasks = new List<ConversionTask>
        {
            new() { SourceFilePath = "a.flac", DestinationFilePath = "out/a.mp3", FileType = FileType.Flac }
        };

        _wrapperMock.Setup(w => w.ConvertFlacToMp3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Throws(new System.Exception("boom"));

        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync(tasks, []);

        Assert.AreEqual(1, result.TotalFiles);
        Assert.AreEqual(1, result.FailedCount);
        _loggerMock.Verify(l => l.LogError(It.Is<string>(s => s.Contains("Exception processing"))), Times.AtLeastOnce);
    }
}

