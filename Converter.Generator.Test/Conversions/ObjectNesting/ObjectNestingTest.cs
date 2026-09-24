using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Generator.Test.Conversions.ObjectNesting;

[TestClass]
public class ObjectNestingTest : ConversionTestBase<ObjectNestingComposition>
{
    public ObjectNestingTest()
        : base(@".\Conversions\ObjectNesting\ObjectNestingComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenEveryNestedLevelGetsItsOwnConverter()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            6,
            GeneratedSources.Count,
            "Four converters are expected for the four nested levels and two for the collection.");
    }

    [TestMethod]
    public void WhenEveryLevelIsFilledThenTheWholeTreeIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ObjectNestingTestCase.From
        {
            Level1 = new ObjectNestingTestCase.From.FromLevel1
            {
                Level2 = new ObjectNestingTestCase.From.FromLevel1.FromLevel2
                {
                    Level3 = new ObjectNestingTestCase.From.FromLevel1.FromLevel2.FromLevel3
                    {
                        Level4 = "end"
                    }
                }
            }
        };

        var to = GetConvertingService().Convert<ObjectNestingTestCase.From, ObjectNestingTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.IsNotNull(to.Level1);
        Assert.IsNotNull(to.Level1.Level2);
        Assert.IsNotNull(to.Level1.Level2.Level3);
        Assert.AreEqual("end", to.Level1.Level2.Level3.Level4);
    }

    [TestMethod]
    public void WhenALevelInTheMiddleIsNullThenTheLevelsAboveAreStillConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ObjectNestingTestCase.From
        {
            Level1 = new ObjectNestingTestCase.From.FromLevel1
            {
                Level2 = new ObjectNestingTestCase.From.FromLevel1.FromLevel2
                {
                    Level3 = null
                }
            }
        };

        var to = GetConvertingService().Convert<ObjectNestingTestCase.From, ObjectNestingTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.IsNotNull(to.Level1);
        Assert.IsNotNull(to.Level1.Level2);
        Assert.IsNull(to.Level1.Level2.Level3);
    }

    [TestMethod]
    public void WhenTheFirstLevelIsNullThenTheTargetIsStillCreated()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ObjectNestingTestCase.From
        {
            Level1 = null
        };

        var to = GetConvertingService().Convert<ObjectNestingTestCase.From, ObjectNestingTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.IsNull(to.Level1);
    }

    [TestMethod]
    public void WhenThePropertyIsACollectionThenEveryItemIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ObjectNestingTestCase.CollectionFrom
        {
            Items = new List<ObjectNestingTestCase.CollectionFrom.FromItem>
            {
                new ObjectNestingTestCase.CollectionFrom.FromItem
                {
                    First = new DateTimeOffset(2020, 01, 01, 01, 01, 01, TimeSpan.Zero),
                    Second = new DateTimeOffset(2021, 01, 01, 01, 01, 01, TimeSpan.Zero)
                },
                new ObjectNestingTestCase.CollectionFrom.FromItem
                {
                    First = new DateTimeOffset(2022, 01, 01, 01, 01, 01, TimeSpan.Zero),
                    Second = new DateTimeOffset(2023, 01, 01, 01, 01, 01, TimeSpan.Zero)
                }
            }
        };

        var to = GetConvertingService()
            .Convert<ObjectNestingTestCase.CollectionFrom, ObjectNestingTestCase.CollectionTo>(from);

        Assert.IsNotNull(to);
        Assert.IsNotNull(to.Items);
        Assert.AreEqual(2, to.Items.Count);

        Assert.AreEqual(from.Items.First().First, to.Items.First().First);
        Assert.AreEqual(from.Items.First().Second, to.Items.First().Second);
        Assert.AreEqual(from.Items.Last().First, to.Items.Last().First);
        Assert.AreEqual(from.Items.Last().Second, to.Items.Last().Second);
    }

    [TestMethod]
    public void WhenTheCollectionIsNullThenTheTargetCollectionIsNull()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ObjectNestingTestCase.CollectionFrom
        {
            Items = null
        };

        var to = GetConvertingService()
            .Convert<ObjectNestingTestCase.CollectionFrom, ObjectNestingTestCase.CollectionTo>(from);

        Assert.IsNotNull(to);
        Assert.IsNull(to.Items);
    }
}
