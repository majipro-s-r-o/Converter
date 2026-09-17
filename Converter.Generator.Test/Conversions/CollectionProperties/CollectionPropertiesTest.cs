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
            3,
            GeneratedSources.Count,
            "One converter for each of the two pairs, plus the item converter the first pair asks "
            + "for once no matter how many of its properties need it.");
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
