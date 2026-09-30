using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.SelfNestedGeneric;

/// <summary>
/// The shape that comes with closed generic types: a pair whose property asks for a bigger pair of
/// the same kind, without end. There is no circle for the queue to notice - every one of those pairs
/// really is a pair nobody has handled - so what ends the travelling is the bound on how deeply a
/// name may nest, and reaching it leaves the property with nothing to fill it.
/// </summary>
/// <remarks>
/// The one case under <c>Validations\</c> whose compilation does produce sources: the converters up
/// to the bound are written on the way down, and the error is reported when the chain reaches it.
/// That the test ends at all is the thing being tested.
/// </remarks>
[TestClass]
public class SelfNestedGenericTest : ConversionTestBase<SelfNestedGenericComposition>
{
    public SelfNestedGenericTest()
        : base(@".\Validations\SelfNestedGeneric\SelfNestedGenericComposition.cs")
    {
    }

    [TestMethod]
    public void WhenAGenericTypeGrowsOutOfItselfThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenAGenericTypeGrowsOutOfItselfThenTheErrorNamesTheProperty()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.Contains(nameof(SelfNestedGenericTestCase.SourceNode<int>.Next)),
            "The property that can not be filled is the one the error is about, but was: " + message);
    }

    [TestMethod]
    public void WhenAGenericTypeGrowsOutOfItselfThenTheGenerationStillEnds()
    {
        Assert.IsTrue(
            GeneratedSources.Count > 1,
            "The converters above the bound are written before the chain reaches it, which is what " +
            "tells this case from a pair that is refused outright.");
    }
}
