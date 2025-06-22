using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Tests;

internal static class DictionaryAssert
{
    internal static void AreEqual<TKey, TExpectedValue, TActualValue>(
        IReadOnlyDictionary<TKey, TExpectedValue> expected,
        IReadOnlyDictionary<TKey, TActualValue> actual,
        IComparer valueComparer)
    {
        Assert.AreEqual(expected.Count, actual.Count, "Dictionary counts do not match.");

        foreach (var kvpExpected in expected)
        {
            bool found = false;
            
            foreach (var kvpActual in actual)
            {
                if (kvpExpected.Key?.Equals(kvpActual.Key) == true &&
                    valueComparer.Compare(kvpExpected.Value, kvpActual.Value) == 0)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                Assert.Fail("Dictionaries are not the same.");
            }
        }
    }

    internal static void AreEqual<TExpectedKey, TExpectedValue, TActualKey, TActualValue>(
        IReadOnlyDictionary<TExpectedKey, TExpectedValue> expected,
        IReadOnlyDictionary<TActualKey, TActualValue> actual,
        IComparer keyComparer,
        IComparer valueComparer)
    {
        Assert.AreEqual(expected.Count, actual.Count, "Dictionary counts do not match.");

        foreach (var kvpExpected in expected)
        {
            bool found = false;
            
            foreach (var kvpActual in actual)
            {
                if (keyComparer.Compare(kvpExpected.Key, kvpActual.Key) == 0 &&
                    valueComparer.Compare(kvpExpected.Value, kvpActual.Value) == 0)
                {
                    found = true;
                    break;
                }
            }

            if (found == false)
            {
                Assert.Fail("Dictionaries are not the same.");
            }
        }
    }
}