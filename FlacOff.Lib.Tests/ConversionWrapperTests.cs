using NUnit.Framework;
using FlacOff.Lib.Services;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Tests.TestHelpers;
using System.IO;

namespace FlacOff.Lib.Tests;

public class ConversionWrapperTests
{
    [SetUp]
    public void Setup()
    {
        // Ensure dry run is cleaned up between tests
        System.Environment.SetEnvironmentVariable("FLACOFF_DRYRUN", null);
    }

    [Test]
    public void ConversionWrapper_ConvertFlacToMp3_DryRun_CreatesDestinationAndReturnsTrue()
    {
        using var tmp = new TestTempDirectory();
        var src = tmp.CreateFile("a.flac", "");
        var dest = Path.Combine(tmp.Path, "out/a.mp3");

        System.Environment.SetEnvironmentVariable("FLACOFF_DRYRUN", "1");
        var logger = new Logger();
        var wrapper = new ConversionWrapper(logger);

        var ok = wrapper.ConvertFlacToMp3(src, dest, "320k");
        Assert.IsTrue(ok);
        Assert.IsTrue(File.Exists(dest));
    }

    [Test]
    public void ConversionWrapper_CopyMp3_DryRun_CreatesDestinationAndReturnsTrue()
    {
        using var tmp = new TestTempDirectory();
        var src = tmp.CreateFile("s.mp3", "abc");
        var dest = Path.Combine(tmp.Path, "out/s.mp3");

        System.Environment.SetEnvironmentVariable("FLACOFF_DRYRUN", "1");
        var logger = new Logger();
        var wrapper = new ConversionWrapper(logger);

        var ok = wrapper.CopyMp3(src, dest);
        Assert.IsTrue(ok);
        Assert.IsTrue(File.Exists(dest));
    }

    [Test]
    public void ConversionWrapper_CopyMp3_RealCopy_CopiesFile()
    {
        using var tmp = new TestTempDirectory();
        var src = tmp.CreateFile("s.mp3", "content");
        var dest = Path.Combine(tmp.Path, "out/s.mp3");

        System.Environment.SetEnvironmentVariable("FLACOFF_DRYRUN", null);
        var logger = new Logger();
        var wrapper = new ConversionWrapper(logger);

        var ok = wrapper.CopyMp3(src, dest);
        Assert.IsTrue(ok);
        Assert.IsTrue(File.Exists(dest));
        Assert.AreEqual("content", File.ReadAllText(dest));
    }
}

