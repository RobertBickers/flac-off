using NUnit.Framework;
using FlacOff.Lib.Utilities;
using FlacOff.Lib.Tests.TestHelpers;
using System.IO;

namespace FlacOff.Lib.Tests;

public class LoggerTests
{
    [Test]
    public void Logger_LogInfo_WithOutputDirectory_WritesToFile()
    {
        using var tmp = new TestTempDirectory();
        var logger = new Logger(tmp.Path);
        logger.LogInfo("test message");

        var logFile = Path.Combine(tmp.Path, "conversion.log");
        Assert.IsTrue(File.Exists(logFile));
        var contents = File.ReadAllText(logFile);
        Assert.IsTrue(contents.Contains("test message"));
    }

    [Test]
    public void Logger_LogMethods_WriteToConsole()
    {
        var sw = new StringWriter();
        var originalOut = System.Console.Out;
        try
        {
            System.Console.SetOut(sw);
            var logger = new Logger();
            logger.LogInfo("info");
            logger.LogWarning("warn");
            logger.LogError("err");

            var output = sw.ToString();
            Assert.IsTrue(output.Contains("info"));
            Assert.IsTrue(output.Contains("warn"));
            Assert.IsTrue(output.Contains("err"));
        }
        finally
        {
            System.Console.SetOut(originalOut);
        }
    }
}

