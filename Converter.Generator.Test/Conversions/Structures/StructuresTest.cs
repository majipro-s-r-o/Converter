using System;

namespace Majipro.Converter.Generator.Test.Conversions.Structures;

[TestClass]
public class StructuresTest : ConversionTestBase<StructuresComposition>
{
    public StructuresTest()
        : base(@".\Conversions\Structures\StructuresComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(5, GeneratedSources.Count, "One converter is expected for each of the five conversions.");
    }

    [TestMethod]
    public void WhenTheTargetPropertyIsNullableThenTheValueIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new StructuresTestCase.ClassWithId
        {
            Id = Guid.NewGuid()
        };

        var to = GetConvertingService()
            .Convert<StructuresTestCase.ClassWithId, StructuresTestCase.ClassWithNullableId>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Id, to.Id);
    }

    [TestMethod]
    public void WhenBothSidesAreStructuresThenTheValueIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new StructuresTestCase.StructWithId
        {
            Id = Guid.NewGuid()
        };

        var to = GetConvertingService()
            .Convert<StructuresTestCase.StructWithId, StructuresTestCase.OtherStructWithId>(from);

        Assert.AreEqual(from.Id, to.Id);
    }

    [TestMethod]
    public void WhenTheTargetIsAStructureThenTheValueIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new StructuresTestCase.ClassSource
        {
            Id = Guid.NewGuid()
        };

        var to = GetConvertingService()
            .Convert<StructuresTestCase.ClassSource, StructuresTestCase.StructTarget>(from);

        Assert.AreEqual(from.Id, to.Id);
    }

    [TestMethod]
    public void WhenTheSourceIsAStructureThenTheValueIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new StructuresTestCase.StructSource
        {
            Id = Guid.NewGuid()
        };

        var to = GetConvertingService()
            .Convert<StructuresTestCase.StructSource, StructuresTestCase.ClassTarget>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Id, to.Id);
    }

    [TestMethod]
    public void WhenOnlyTheNullableAnnotationDiffersThenThePropertyIsStillConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new StructuresTestCase.RecordWithAnnotatedId
        {
            Id = "hello"
        };

        var to = GetConvertingService()
            .Convert<StructuresTestCase.RecordWithAnnotatedId, StructuresTestCase.ClassWithStringId>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Id, to.Id);
    }
}
