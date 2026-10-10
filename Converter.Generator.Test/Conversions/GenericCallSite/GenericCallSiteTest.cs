using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Conversions.GenericCallSite;

/// <summary>
/// A generic method handing its own type parameter to the converting service, and nothing calling
/// it. Nothing can be generated for such a pair - <c>TFrom</c> means something only inside the method
/// it belongs to, and writing a converter that names it would break the build of the consuming
/// project - so the call site is reported as a warning, and the rest is still generated.
/// </summary>
[TestClass]
public class GenericCallSiteTest : ConversionTestBase<GenericCallSiteComposition>
{
    public GenericCallSiteTest()
        : base(@".\Conversions\GenericCallSite\GenericCallSiteComposition.cs")
    {
    }

    [TestMethod]
    public void WhenNothingClosesTheCallSiteThenAWarningIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0005", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), nameof(GenericCallSiteComposition.ConvertToString));
        Assert.IsTrue(diagnostic.Location.IsInSource, "The warning is expected on the call site.");
    }

    [TestMethod]
    public void WhenGeneratorRunsThenOnlyThePairWhoseTypesAreWrittenOutIsGenerated()
    {
        Assert.AreEqual(
            1,
            GeneratedSources.Count,
            "Only the pair whose types are written out is expected.");
    }

    [TestMethod]
    public void WhenACallSiteIsGenericThenItIsSkippedAndTheRestIsStillGenerated()
    {
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
