namespace Majipro.Converter.Generator.Test.Conversions.ImplementedConverters;

[TestClass]
public class ImplementedConvertersTest : ConversionTestBase<ImplementedConvertersComposition>
{
    public ImplementedConvertersTest()
        : base(@".\Conversions\ImplementedConverters\ImplementedConvertersComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            1,
            GeneratedSources.Count,
            "Only the pair nobody wrote a converter for is expected to be generated.");
    }

    [TestMethod]
    public void WhenOnlyTheValueConversionIsWrittenThenTheWrittenOneIsUsed()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ImplementedConvertersTestCase.WrittenConverterFrom
        {
            Name = "hello"
        };

        var to = GetConvertingService()
            .Convert<ImplementedConvertersTestCase.WrittenConverterFrom,
                ImplementedConvertersTestCase.WrittenConverterTo>(from);

        Assert.AreEqual(WrittenConverter.Marker + from.Name, to.Name);
    }

    [TestMethod]
    public void WhenOnlyTheValueConversionIsWrittenThenTheReferenceOneIsNotGenerated()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter = FindReferenceConverter<ImplementedConvertersTestCase.WrittenConverterFrom,
            ImplementedConvertersTestCase.WrittenConverterTo>();

        Assert.IsNull(
            converter,
            "Generating the missing half would claim the half that was written, so the pair is left alone.");
    }

    [TestMethod]
    public void WhenTheReferenceConversionIsWrittenThenTheWrittenOneIsUsed()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ImplementedConvertersTestCase.WrittenReferenceFrom
        {
            Name = "hello"
        };

        var to = new ImplementedConvertersTestCase.WrittenReferenceTo();

        var converted = GetConvertingService()
            .Convert<ImplementedConvertersTestCase.WrittenReferenceFrom,
                ImplementedConvertersTestCase.WrittenReferenceTo>(from, to);

        Assert.AreSame(to, converted);
        Assert.AreEqual(WrittenReferenceConverter.Marker + from.Name, to.Name);
    }

    [TestMethod]
    public void WhenTheReferenceConversionIsWrittenThenItsValueConversionIsUsedAsWell()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ImplementedConvertersTestCase.WrittenReferenceFrom
        {
            Name = "hello"
        };

        var to = GetConvertingService()
            .Convert<ImplementedConvertersTestCase.WrittenReferenceFrom,
                ImplementedConvertersTestCase.WrittenReferenceTo>(from);

        Assert.AreEqual(WrittenReferenceConverter.Marker + from.Name, to.Name);
    }

    [TestMethod]
    public void WhenNothingIsWrittenThenBothConversionsAreGenerated()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new ImplementedConvertersTestCase.GeneratedFrom
        {
            Name = "hello"
        };

        var to = new ImplementedConvertersTestCase.GeneratedTo();

        var converted = GetConvertingService()
            .Convert<ImplementedConvertersTestCase.GeneratedFrom, ImplementedConvertersTestCase.GeneratedTo>(from, to);

        Assert.AreSame(to, converted);
        Assert.AreEqual(from.Name, to.Name);
    }
}
