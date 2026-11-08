using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.UnconvertibleProperty;

/// <summary>
/// The other half of MC0003: the target property has a source property of the same name, and no
/// value rule claims the pair. <c>double -&gt; float</c> is the shape that reaches this today - the
/// compiler does not narrow by itself and nothing answers the pair - and the honest answer is the
/// same as for a property that is not there at all.
/// </summary>
[TestClass]
public class UnconvertiblePropertyTest : ConversionTestBase<UnconvertiblePropertyComposition>
{
    public UnconvertiblePropertyTest()
        : base(@".\Validations\UnconvertibleProperty\UnconvertiblePropertyComposition.cs")
    {
    }

    [TestMethod]
    public void WhenNoValueRuleClaimsThePropertyThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenNoValueRuleClaimsThePropertyThenTheErrorNamesIt()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.EndsWith(": " + nameof(UnconvertiblePropertyTestCase.To.Amount)),
            "The listed properties are expected to be exactly the ones nothing fills, but were: " + message);
    }

    [TestMethod]
    public void WhenNoValueRuleClaimsThePropertyThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
