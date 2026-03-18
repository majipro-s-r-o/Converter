using System.Collections;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Comparers;

internal sealed class HasIntegerComparer : IComparer
{
    public int Compare(object x, object y)
    {
        var a = x as IHasInteger;
        var b = y as IHasInteger;

        if (a == null || b == null) return -1;

        return a.Integer == b.Integer ? 0 : -1;
    }
}
