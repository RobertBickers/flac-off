using NUnit.Framework;
using System;

namespace FlacOff.Lib.Tests;

public class AssemblySetup
{
    private string? _old;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _old = Environment.GetEnvironmentVariable("FLACOFF_SILENT_PROGRESS");
        Environment.SetEnvironmentVariable("FLACOFF_SILENT_PROGRESS", "1");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Environment.SetEnvironmentVariable("FLACOFF_SILENT_PROGRESS", _old);
    }
}

