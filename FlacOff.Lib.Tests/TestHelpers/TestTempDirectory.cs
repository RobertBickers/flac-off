using System;
using System.IO;

namespace FlacOff.Lib.Tests.TestHelpers;

public class TestTempDirectory : IDisposable
{
    public string Path { get; }

    private readonly string _originalCwd;

    public TestTempDirectory()
    {
        _originalCwd = Environment.CurrentDirectory;
        Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FlacOffTests_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path);
    }

    public string CreateFile(string relativePath, string content = "test")
    {
        var full = System.IO.Path.Combine(Path, relativePath);
        var dir = System.IO.Path.GetDirectoryName(full);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir)) Directory.CreateDirectory(dir);
        File.WriteAllText(full, content);
        return full;
    }

    public void SetAsCurrentDirectory()
    {
        Environment.CurrentDirectory = Path;
    }

    public void Dispose()
    {
        try
        {
            Environment.CurrentDirectory = _originalCwd;
            if (Directory.Exists(Path)) Directory.Delete(Path, recursive: true);
        }
        catch
        {
            // ignore cleanup failures
        }
    }
}

