using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Tests;

public class HashSetAssert
{
    public static void AreEqual<TFrom, TTo>(ISet<TFrom> expected, ISet<TTo> actual, IComparer comparer)
    {
        CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), comparer);
    }
}