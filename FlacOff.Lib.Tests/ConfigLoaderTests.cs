using NUnit.Framework;
using FlacOff.Lib.Config;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Tests.TestHelpers;
using System.IO;
using System.Text.Json;

namespace FlacOff.Lib.Tests;

public class ConfigLoaderTests
{
    [Test]
    public void ConfigLoader_LoadConfig_ValidFile_ReturnsAppConfig()
    {
        using var tmp = new TestTempDirectory();
        tmp.SetAsCurrentDirectory();

        var json = JsonSerializer.Serialize(new
        {
            InputDirectory = "./input",
            OutputDirectory = "./output",
            UseParallelProcessing = false,
            MaxDegreeOfParallelism = 2,
            Mp3Bitrate = "192k"
        });

        File.WriteAllText(Path.Combine(tmp.Path, "appsettings.json"), json);
        Directory.CreateDirectory(Path.Combine(tmp.Path, "input"));

        var logger = new Logger();
        var loader = new ConfigLoader(logger);
        var config = loader.LoadConfig();

        Assert.IsNotNull(config);
        var expectedInput = Path.GetFullPath(Path.Combine(tmp.Path, "input"));
        var actualInput = Path.GetFullPath(config.InputDirectory);

        // Accept either /var/... or /private/var/... variants on macOS by allowing either normalized form
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedInput, actualInput), $"Expected: {expectedInput} Actual: {actualInput}");

        var expectedOutput = Path.GetFullPath(Path.Combine(tmp.Path, "output"));
        var actualOutput = Path.GetFullPath(config.OutputDirectory);
        Assert.IsTrue(TestPathHelper.ArePathsEquivalent(expectedOutput, actualOutput), $"Expected: {expectedOutput} Actual: {actualOutput}");
        Assert.AreEqual("192k", config.Mp3Bitrate);
        Assert.AreEqual(2, config.MaxDegreeOfParallelism);
    }

    [Test]
    public void ConfigLoader_LoadConfig_MissingFile_ThrowsFileNotFoundException()
    {
        using var tmp = new TestTempDirectory();
        tmp.SetAsCurrentDirectory();

        var logger = new Logger();
        var loader = new ConfigLoader(logger);

        Assert.Throws<FileNotFoundException>(() => loader.LoadConfig());
    }

    [Test]
    public void ConfigLoader_LoadConfig_InvalidJson_ThrowsException()
    {
        using var tmp = new TestTempDirectory();
        tmp.SetAsCurrentDirectory();

        File.WriteAllText(Path.Combine(tmp.Path, "appsettings.json"), "{ invalid json");
        var logger = new Logger();
        var loader = new ConfigLoader(logger);

        Assert.Throws<System.Text.Json.JsonException>(() => loader.LoadConfig());
    }

    [Test]
    public void ConfigLoader_LoadConfig_TildePath_ExpandsToUserHome()
    {
        using var tmp = new TestTempDirectory();
        tmp.SetAsCurrentDirectory();

        var json = $"{{ \"InputDirectory\": \"~/\", \"OutputDirectory\": \"./out\" }}";
        File.WriteAllText(Path.Combine(tmp.Path, "appsettings.json"), json);

        var logger = new Logger();
        var loader = new ConfigLoader(logger);
        var config = loader.LoadConfig();

        var home = System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile);
        Assert.AreEqual(home, config.InputDirectory);
    }
}