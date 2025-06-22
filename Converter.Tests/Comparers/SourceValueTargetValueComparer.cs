using System.Collections;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Comparers;

internal sealed class SourceValueTargetValueComparer : IComparer
{
    public int Compare(object x, object y)
    {
        var a = x as SourceValue;
        var b = y as TargetValue;

        if (a == null || b == null) return -1;

        return a.Integer == b.Integer ? 0 : -1;
    }
}