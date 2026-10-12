using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Conversions.MarkedGenericCallSite;

/// <summary>
/// A generic call site inside a method marked with <c>RequiresConverterAttribute</c> is not
/// unresolved when the pair it asks for is the one the method declares: every call of the method
/// requests the closed pair, in whatever assembly it is. So <c>MC0005</c> is reported only for the one
/// call site asking for a pair its method does not declare.
/// </summary>
[TestClass]
public class MarkedGenericCallSiteTest : ConversionTestBase<MarkedGenericCallSiteComposition>
{
    public MarkedGenericCallSiteTest()
        : base(@".\Conversions\MarkedGenericCallSite\MarkedGenericCallSiteComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheMethodDeclaresThePairThenTheCallSiteIsNotReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0005", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Warning, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenTheMethodDoesNotDeclareThePairThenTheCallSiteIsStillReported()
    {
        var message = Diagnostic.Single().GetMessage();

        StringAssert.Contains(message, nameof(MarkedGenericCallSiteComposition.NotDeclared));
    }

    [TestMethod]
    public void WhenGeneratorRunsThenOnlyThePairWhoseTypesAreWrittenOutIsGenerated()
    {
        Assert.AreEqual(1, GeneratedSources.Count, "Only the pair whose types are written out is expected.");
    }

    [TestMethod]
    public void WhenGeneratorRunsThenTheOrdinaryPairStillConverts()
    {
        var to = GetConvertingService()
            .Convert<MarkedGenericCallSiteTestCase.From, MarkedGenericCallSiteTestCase.To>(
                new MarkedGenericCallSiteTestCase.From { Property = "hello" });

        Assert.AreEqual("hello", to.Property);
    }
}
