using System.Collections;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Comparers;

internal sealed class SourceTargetComparer : IComparer
{
    public int Compare(object x, object y)
    {
        var a = x as Source;
        var b = y as Target;

        if (a == null || b == null) return -1;

        return a.Integer == b.Integer ? 0 : -1;
    }
}