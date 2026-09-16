using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Generator.Test.Conversions.CollectionProperties;

[TestClass]
public class CollectionPropertiesTest : ConversionTestBase<CollectionPropertiesComposition>
{
    public CollectionPropertiesTest()
        : base(@".\Conversions\CollectionProperties\CollectionPropertiesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenTheItemConverterIsWrittenOnce()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            5,
            GeneratedSources.Count,
            "One converter for each of the three pairs, plus the item converter the first pair asks "
            + "for once no matter how many of its properties need it, plus the one that writes an "
            + "item out as text. Reading an item back needs none - the library has that converter.");
    }

    [TestMethod]
    public void WhenTargetIsAListThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.ListItems);
    }

    [TestMethod]
    public void WhenTargetIsAListInterfaceThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.ListInterfaceItems);
    }

    [TestMethod]
    public void WhenTargetIsACollectionThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.CollectionItems);
    }

    [TestMethod]
    public void WhenTargetIsAnEnumerableThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.EnumerableItems);
    }

    [TestMethod]
    public void WhenTargetIsAReadOnlyCollectionThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.ReadOnlyCollectionItems);
    }

    [TestMethod]
    public void WhenSourceIsAnArrayThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = Convert();

        AssertItems(to.ArrayItems);
    }

    [TestMethod]
    public void WhenItemsAreTheSameTypeOnBothSidesThenTheCollectionIsRebuilt()
    {
        AssertGeneratedWithoutDiagnostics();

        var ids = new List<Guid>
        {
            Guid.NewGuid(),
            Guid.NewGuid()
        };

        var from = new CollectionPropertiesTestCase.SameItemsFrom
        {
            Tags = new[] { "first", "second" },
            Ids = ids
        };

        var to = GetConvertingService()
            .Convert<CollectionPropertiesTestCase.SameItemsFrom, CollectionPropertiesTestCase.SameItemsTo>(from);

        Assert.IsNotNull(to);
        CollectionAssert.AreEqual(from.Tags, to.Tags);
        CollectionAssert.AreEqual(ids, to.Ids);

        Assert.AreNotSame(
            ids,
            (object)to.Ids,
            "The source list is assignable to nothing the target declares, so the items go through "
            + "the converting service and land in a list of their own.");
    }

    /// <summary>
    /// The item pair is the one thing that decides whether a collection can be written, and a
    /// scalar and its text is a pair like any other - the same one a property of those two types
    /// would be, down to what the built-in converter makes of an item that is not a number.
    /// </summary>
    [TestMethod]
    public void WhenTheItemsAreAScalarAndItsTextThenEveryItemGoesThroughItsConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new CollectionPropertiesTestCase.ScalarItemsFrom
        {
            Counts = new List<string> { "1", "2", "not a number" },
            Numbers = new List<int> { 3, 4 }
        };

        var to = GetConvertingService()
            .Convert<CollectionPropertiesTestCase.ScalarItemsFrom, CollectionPropertiesTestCase.ScalarItemsTo>(from);

        Assert.IsNotNull(to);

        CollectionAssert.AreEqual(
            new[] { 1, 2, 0 },
            to.Counts,
            "Every item is read by the built-in converter, the one that is not a number included.");

        CollectionAssert.AreEqual(new[] { "3", "4" }, to.Numbers);
    }

    [TestMethod]
    public void WhenTheScalarItemsAreNullThenTheCollectionStaysNull()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<CollectionPropertiesTestCase.ScalarItemsFrom, CollectionPropertiesTestCase.ScalarItemsTo>(
                new CollectionPropertiesTestCase.ScalarItemsFrom());

        Assert.IsNotNull(to);
        Assert.IsNull(to.Counts);
        Assert.IsNull(to.Numbers);
    }

    private CollectionPropertiesTestCase.To Convert()
    {
        var items = new List<CollectionPropertiesTestCase.From.FromItem>
        {
            new CollectionPropertiesTestCase.From.FromItem
            {
                Value = "first"
            },
            new CollectionPropertiesTestCase.From.FromItem
            {
                Value = "second"
            }
        };

        var from = new CollectionPropertiesTestCase.From
        {
            ListItems = items,
            ListInterfaceItems = items,
            CollectionItems = items,
            EnumerableItems = items,
            ReadOnlyCollectionItems = items,
            ArrayItems = items.ToArray()
        };

        return GetConvertingService()
            .Convert<CollectionPropertiesTestCase.From, CollectionPropertiesTestCase.To>(from);
    }

    private static void AssertItems(IEnumerable<CollectionPropertiesTestCase.To.ToItem> actual)
    {
        Assert.IsNotNull(actual);

        var items = actual.ToList();

        Assert.AreEqual(2, items.Count);
        Assert.AreEqual("first", items[0].Value);
        Assert.AreEqual("second", items[1].Value);
    }
}
