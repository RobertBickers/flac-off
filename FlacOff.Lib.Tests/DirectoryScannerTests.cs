using NUnit.Framework;
using FlacOff.Lib.Services;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Tests.TestHelpers;
using System.IO;
using System.Linq;

namespace FlacOff.Lib.Tests;

public class DirectoryScannerTests
{
    [Test]
    public void DirectoryScanner_ScanDirectory_WithFlacAndMp3Files_ReturnsCorrectTasks()
    {
        using var tmp = new TestTempDirectory();
        var input = Path.Combine(tmp.Path, "music");
        var output = Path.Combine(tmp.Path, "out");
        Directory.CreateDirectory(input);
        Directory.CreateDirectory(output);

        var albumDir = Path.Combine(input, "Artist/Album");
        Directory.CreateDirectory(albumDir);
        File.WriteAllText(Path.Combine(albumDir, "track1.flac"), "");
        File.WriteAllText(Path.Combine(albumDir, "booklet.mp3"), "");

        var logger = new Logger();
        var scanner = new DirectoryScanner(logger);
        var (flacTasks, mp3Tasks) = scanner.ScanDirectory(input, output);

        Assert.AreEqual(1, flacTasks.Count);
        Assert.AreEqual(1, mp3Tasks.Count);

        var flac = flacTasks.First();
        var expectedFlacSource = Path.Combine(albumDir, "track1.flac");
        var expectedFlacDest = Path.Combine(output, "Artist", "Album", "track1.mp3");
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedFlacSource, flac.SourceFilePath), $"Expected flac source {expectedFlacSource} but was {flac.SourceFilePath}");
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedFlacDest, flac.DestinationFilePath), $"Expected flac dest {expectedFlacDest} but was {flac.DestinationFilePath}");

        var mp3 = mp3Tasks.First();
        var expectedMp3Source = Path.Combine(albumDir, "booklet.mp3");
        var expectedMp3Dest = Path.Combine(output, "Artist", "Album", "booklet.mp3");
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedMp3Source, mp3.SourceFilePath), $"Expected mp3 source {expectedMp3Source} but was {mp3.SourceFilePath}");
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedMp3Dest, mp3.DestinationFilePath), $"Expected mp3 dest {expectedMp3Dest} but was {mp3.DestinationFilePath}");
    }

    [Test]
    public void DirectoryScanner_ScanDirectory_EmptyDirectory_ReturnsEmptyLists()
    {
        using var tmp = new TestTempDirectory();
        var input = Path.Combine(tmp.Path, "empty");
        var output = Path.Combine(tmp.Path, "out");
        Directory.CreateDirectory(input);
        Directory.CreateDirectory(output);

        var logger = new Logger();
        var scanner = new DirectoryScanner(logger);
        var (flacTasks, mp3Tasks) = scanner.ScanDirectory(input, output);

        Assert.IsEmpty(flacTasks);
        Assert.IsEmpty(mp3Tasks);
    }

    [Test]
    public void DirectoryScanner_GetDetectedFolders_MixedTasks_ReturnsSortedUniqueFolders()
    {
        using var tmp = new TestTempDirectory();
        var input = Path.Combine(tmp.Path, "music");
        var output = Path.Combine(tmp.Path, "out");
        Directory.CreateDirectory(input);
        Directory.CreateDirectory(output);

        var dirA = Path.Combine(input, "A");
        var dirB = Path.Combine(input, "B");
        Directory.CreateDirectory(dirA);
        Directory.CreateDirectory(dirB);
        File.WriteAllText(Path.Combine(dirA, "one.flac"), "");
        File.WriteAllText(Path.Combine(dirB, "two.mp3"), "");

        var logger = new Logger();
        var scanner = new DirectoryScanner(logger);
        var (flacTasks, mp3Tasks) = scanner.ScanDirectory(input, output);

        var folders = scanner.GetDetectedFolders(flacTasks, mp3Tasks);
        Assert.AreEqual(2, folders.Count);
        Assert.AreEqual(2, folders.Distinct().Count());
    }
}
