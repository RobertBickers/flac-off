using NUnit.Framework;
using Moq;
using FlacOff.Lib.Services;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Config;
using FlacOff.Lib.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FlacOff.Lib.Tests;

public class ConversionServiceProgressTests
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
    public async Task ConversionService_Sequential_EmitsProgressViaLogger()
    {
        var tasks = new List<ConversionTask>
        {
            new() { SourceFilePath = "a.flac", DestinationFilePath = "out/a.mp3", FileType = FileType.Flac }
        };

        _wrapperMock.Setup(w => w.ConvertFlacToMp3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync(tasks, []);

        _loggerMock.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Converting"))), Times.AtLeastOnce);
    }

    [Test]
    public async Task ConversionService_Parallel_EmitsProgressViaLogger()
    {
        _config.UseParallelProcessing = true;
        var tasks = new List<ConversionTask>
        {
            new() { SourceFilePath = "a.flac", DestinationFilePath = "out/a.mp3", FileType = FileType.Flac },
            new() { SourceFilePath = "b.flac", DestinationFilePath = "out/b.mp3", FileType = FileType.Flac }
        };

        _wrapperMock.Setup(w => w.ConvertFlacToMp3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        var service = new ConversionService(_wrapperMock.Object, _loggerMock.Object, _config);
        var result = await service.ConvertAsync(tasks, []);

        _loggerMock.Verify(l => l.LogInfo(It.Is<string>(s => s.Contains("Converting"))), Times.AtLeastOnce);
    }
}

