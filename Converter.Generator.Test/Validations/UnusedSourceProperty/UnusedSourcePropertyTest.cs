using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.UnusedSourceProperty;

/// <summary>
/// The source has a property of its own and the target has one nothing fills. Only the second one
/// is a problem: a property the source knows and the target does not is dropped, which loses
/// nothing the target was ever going to carry, while the other direction leaves the target half
/// built. Having one of each does not make them cancel out.
/// </summary>
[TestClass]
public class UnusedSourcePropertyTest : ConversionTestBase<UnusedSourcePropertyComposition>
{
    public UnusedSourcePropertyTest()
        : base(@".\Validations\UnusedSourceProperty\UnusedSourcePropertyComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheSourcePropertyIsUnusedThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    /// <summary>
    /// The unused source property is not the complaint, the target property nothing fills is.
    /// </summary>
    [TestMethod]
    public void WhenTheSourcePropertyIsUnusedThenTheErrorNamesTheTargetProperty()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.EndsWith(": " + nameof(UnusedSourcePropertyTestCase.To.B)),
            "The listed properties are expected to be exactly the ones nothing fills, but were: " + message);
    }

    [TestMethod]
    public void WhenTheSourcePropertyIsUnusedThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
