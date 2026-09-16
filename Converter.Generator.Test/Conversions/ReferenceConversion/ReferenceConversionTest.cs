using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Generator.Test.Conversions.ReferenceConversion;

[TestClass]
public class ReferenceConversionTest : ConversionTestBase<ReferenceConversionComposition>
{
    public ReferenceConversionTest()
        : base(@".\Conversions\ReferenceConversion\ReferenceConversionComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            2,
            GeneratedSources.Count,
            "One converter is expected for the pair and one for the nested property.");
    }

    [TestMethod]
    public void WhenTheConverterIsGeneratedThenItIsBothConversionsInOneClass()
    {
        AssertGeneratedWithoutDiagnostics();

        var converter =
            GetReferenceConverter<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>();

        Assert.IsInstanceOfType<IConverter<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>>(
            converter,
            "A reference converter is expected to be a converter as well, so the pair is registered once.");
    }

    [TestMethod]
    public void WhenThereIsNoTargetThenANewOneIsCreated()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetFrom();

        var to = GetConvertingService()
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Name, to.Name);
        Assert.AreEqual(from.Nested.Number, to.Nested.Number);
    }

    [TestMethod]
    public void WhenTheTargetIsGivenThenItIsTheOneThatComesBack()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetFrom();
        var to = new ReferenceConversionTestCase.To();

        var converted = GetConvertingService()
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(from, to);

        Assert.AreSame(to, converted, "The instance the caller holds is the one that is enhanced.");
        Assert.AreEqual(from.Name, to.Name);
        Assert.AreEqual(from.Nested.Number, to.Nested.Number);
    }

    [TestMethod]
    public void WhenTheTargetIsGivenThenWhatTheGeneratorDoesNotSeeIsLeftAlone()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = new ReferenceConversionTestCase.To
        {
            Untouched = "kept"
        };

        GetConvertingService()
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(GetFrom(), to);

        Assert.AreEqual("kept", to.Untouched);
    }

    [TestMethod]
    public void WhenTheSourceIsNullThenTheTargetComesBackUnchanged()
    {
        AssertGeneratedWithoutDiagnostics();

        var to = new ReferenceConversionTestCase.To
        {
            Name = "unchanged"
        };

        var converted = GetConvertingService()
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(null, to);

        Assert.AreSame(to, converted);
        Assert.AreEqual("unchanged", to.Name, "There is nothing to enhance the target with.");
    }

    [TestMethod]
    public void WhenTheTargetIsGivenThenANestedPropertyIsConvertedAsWell()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = GetFrom();

        var to = new ReferenceConversionTestCase.To
        {
            Nested = new ReferenceConversionTestCase.NestedTarget
            {
                Number = -1
            }
        };

        GetConvertingService()
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(from, to);

        Assert.AreEqual(
            from.Nested.Number,
            to.Nested.Number,
            "A property the generator fills is overwritten, nested or not.");
    }

    private static ReferenceConversionTestCase.From GetFrom()
    {
        return new ReferenceConversionTestCase.From
        {
            Name = "hello",
            Nested = new ReferenceConversionTestCase.Nested
            {
                Number = 42
            }
        };
    }
}
