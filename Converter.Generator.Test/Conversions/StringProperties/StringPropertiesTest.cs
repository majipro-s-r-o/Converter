using System;

namespace Majipro.Converter.Generator.Test.Conversions.StringProperties;

/// <summary>
/// A property that is a scalar on one side and its text on the other. Both directions are written
/// as a call to the converting service, so what actually converts is a converter: a generated one
/// on the way out, a built-in one of <c>Converter\Converters</c> on the way back.
/// </summary>
[TestClass]
public class StringPropertiesTest : ConversionTestBase<StringPropertiesComposition>
{
    public StringPropertiesTest()
        : base(@".\Conversions\StringProperties\StringPropertiesComposition.cs")
    {
    }

    /// <summary>
    /// Two converters for the two pairs and one for each scalar being written out. The five pairs
    /// of the other direction add nothing: no body rule knows how to parse, and none has to - the
    /// library registers a converter for every one of them.
    /// </summary>
    [TestMethod]
    public void WhenTheConverterIsServedByTheLibraryThenNothingIsGeneratedForIt()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            8,
            GeneratedSources.Count,
            "Expected the two pairs plus one converter per scalar written out, and nothing for a "
            + "pair Converter\\Converters already answers.");
    }

    [TestMethod]
    public void WhenTheTargetPropertyIsAStringThenTheScalarIsWrittenOut()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetScalarFrom();

        var to = GetConvertingService()
            .Convert<StringPropertiesTestCase.ScalarFrom, StringPropertiesTestCase.TextTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(Convert.ToString(from.Integer), to.Integer);
        Assert.AreEqual(Convert.ToString(from.Decimal), to.Decimal);
        Assert.AreEqual(Convert.ToString(from.Bool), to.Bool);
        Assert.AreEqual(Convert.ToString(from.Id), to.Id);
        Assert.AreEqual(nameof(StringPropertiesTestCase.Status.Active), to.Status);
        Assert.AreEqual(Convert.ToString(from.NullableInteger), to.NullableInteger);
    }

    [TestMethod]
    public void WhenTheScalarIsANullableWithoutAValueThenTheTextIsNull()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetScalarFrom();
        from.NullableInteger = null;

        var to = GetConvertingService()
            .Convert<StringPropertiesTestCase.ScalarFrom, StringPropertiesTestCase.TextTo>(from);

        Assert.IsNotNull(to);
        Assert.IsNull(to.NullableInteger, "There is no value to write out, and no text for it either.");
    }

    [TestMethod]
    public void WhenTheSourcePropertyIsAStringThenTheBuiltInConverterReadsIt()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = GetConvertingService()
            .Convert<StringPropertiesTestCase.TextFrom, StringPropertiesTestCase.ScalarTo>(GetTextFrom());

        Assert.IsNotNull(to);
        Assert.AreEqual(42, to.Integer);
        Assert.AreEqual(9000000000L, to.Long);
        Assert.AreEqual(1234.56m, to.Decimal);
        Assert.IsTrue(to.Bool);
        Assert.AreEqual(new DateTime(2024, 1, 31), to.Created);
    }

    /// <summary>
    /// What an unparseable string turns into is the built-in converter's answer and not this
    /// generator's, which is the point of routing the property through it.
    /// </summary>
    [TestMethod]
    public void WhenTheStringIsNotAValueThenTheBuiltInConverterDecidesWhatItBecomes()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetTextFrom();
        from.Integer = "not a number";

        var to = GetConvertingService()
            .Convert<StringPropertiesTestCase.TextFrom, StringPropertiesTestCase.ScalarTo>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(0, to.Integer);
    }

    private static StringPropertiesTestCase.ScalarFrom GetScalarFrom()
    {
        return new StringPropertiesTestCase.ScalarFrom
        {
            Integer = 42,
            Decimal = 1234.56m,
            Bool = true,
            Id = Guid.NewGuid(),
            Status = StringPropertiesTestCase.Status.Active,
            NullableInteger = 7
        };
    }

    /// <summary>
    /// Written the way the invariant culture reads them, which is what
    /// <c>ConverterOptions.FormatProvider</c> defaults to.
    /// </summary>
    private static StringPropertiesTestCase.TextFrom GetTextFrom()
    {
        return new StringPropertiesTestCase.TextFrom
        {
            Integer = "42",
            Long = "9000000000",
            Decimal = "1234.56",
            Bool = "true",
            Created = "2024-01-31"
        };
    }
}
