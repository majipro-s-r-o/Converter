using System;
using System.IO;

namespace Majipro.Converter.Generator.Test.Extensions;

internal static class StringExtensions
{
    /// <summary>
    /// Turns a Windows style relative path of a test case file into an absolute path next to the
    /// test assembly, where files marked as <c>CopyToOutputDirectory</c> land.
    /// </summary>
    internal static string ToPath(this string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return path;
        }

        var fragments = path.Split('\\');

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, Path.Combine(fragments)));
    }
}
