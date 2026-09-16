using System;

namespace Majipro.Converter.Generator.Test.Conversions.NullableTypes;

[TestClass]
public class NullableTypesTest : ConversionTestBase<NullableTypesComposition>
{
    public NullableTypesTest()
        : base(@".\Conversions\NullableTypes\NullableTypesComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(5, GeneratedSources.Count, "One converter is expected for each of the five conversions.");
    }

    [TestMethod]
    public void WhenTheTargetIsAStringThenANullablePrimitiveIsWrittenOut()
    {
        AssertGeneratedWithoutDiagnostics();

        int? from = 1;

        var to = GetConvertingService().Convert<int?, string>(from);

        Assert.AreEqual(from.ToString(), to);
    }

    [TestMethod]
    public void WhenTheTargetIsAStringThenANullableStructureIsWrittenOut()
    {
        AssertGeneratedWithoutDiagnostics();

        Guid? from = Guid.NewGuid();

        var to = GetConvertingService().Convert<Guid?, string>(from);

        Assert.AreEqual(from.ToString(), to);
    }

    [TestMethod]
    public void WhenOnlyTheSourceIsNullableThenItsValueIsConverted()
    {
        AssertGeneratedWithoutDiagnostics();

        NullableTypesTestCase.NullableSourceFrom? from = new NullableTypesTestCase.NullableSourceFrom
        {
            SomeValue = "some-value"
        };

        var to = GetConvertingService()
            .Convert<NullableTypesTestCase.NullableSourceFrom?, NullableTypesTestCase.NullableSourceTo>(from);

        Assert.AreEqual(from.Value.SomeValue, to.SomeValue);
    }

    [TestMethod]
    public void WhenBothSidesAreNullableThenANullSourceStaysNull()
    {
        AssertGeneratedWithoutDiagnostics();

        NullableTypesTestCase.NullableBothFrom? from = null;

        var to = GetConvertingService()
            .Convert<NullableTypesTestCase.NullableBothFrom?, NullableTypesTestCase.NullableBothTo?>(from);

        Assert.IsNull(to);
    }

    [TestMethod]
    public void WhenOnlyTheTargetIsNullableThenItCarriesTheConvertedValue()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new NullableTypesTestCase.NullableTargetFrom
        {
            SomeValue = "some-value"
        };

        var to = GetConvertingService()
            .Convert<NullableTypesTestCase.NullableTargetFrom, NullableTypesTestCase.NullableTargetTo?>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.SomeValue, to.Value.SomeValue);
    }
}
