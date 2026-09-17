namespace Majipro.Converter.Generator.Test.Conversions.GenericCallSite;

/// <summary>
/// A generic method handing its own type parameter to the converting service. Nothing can be
/// generated for such a pair - <c>TFrom</c> means something only inside the method it belongs to -
/// and writing a converter that names it would break the build of the consuming project.
/// </summary>
[TestClass]
public class GenericCallSiteTest : ConversionTestBase<GenericCallSiteComposition>
{
    public GenericCallSiteTest()
        : base(@".\Conversions\GenericCallSite\GenericCallSiteComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(
            1,
            GeneratedSources.Count,
            "Only the pair whose types are written out is expected.");
    }

    [TestMethod]
    public void WhenACallSiteIsGenericThenItIsSkippedAndTheRestIsStillGenerated()
    {
        AssertGeneratedWithoutDiagnostics();

        var from = new GenericCallSiteTestCase.From
        {
            Property = "hello"
        };

        var to = GetConvertingService()
            .Convert<GenericCallSiteTestCase.From, GenericCallSiteTestCase.To>(from);

        Assert.IsNotNull(to);
        Assert.AreEqual(from.Property, to.Property);
    }
}
