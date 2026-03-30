using System;
using System.IO;

namespace FlacOff.Lib.Tests.TestHelpers;

public static class TestPathHelper
{
    // Normalize a path for comparison by resolving to full path and converting known macOS private/var symlink differences
    public static string NormalizeForComparison(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return path ?? string.Empty;
        var full = Path.GetFullPath(path);

        // On macOS, /var is a symlink to /private/var; normalize both to /private/var for comparison
        if (full.StartsWith("/var/", StringComparison.Ordinal))
        {
            full = full.Replace("/var/", "/private/var/");
        }

        // Normalize directory separators on Windows to use backslash and perform case-insensitive comparison
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            full = full.Replace('/', '\\');
            full = full.ToLowerInvariant();
        }
        else
        {
            // Normalize mixed separators on POSIX
            full = full.Replace('\\', '/');
        }

        return full;
    }

    public static bool ArePathsEquivalent(string a, string b)
    {
        var na = NormalizeForComparison(a);
        var nb = NormalizeForComparison(b);
        return string.Equals(na, nb, StringComparison.Ordinal);
    }
}
