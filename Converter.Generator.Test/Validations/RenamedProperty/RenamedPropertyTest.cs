using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.RenamedProperty;

/// <summary>
/// The property was renamed on the way to the target. The generator matches by name and nothing
/// else, so a rename is indistinguishable from a property that was never there - and guessing that
/// <c>B</c> was meant to become <c>BB</c> is exactly what it must not do.
/// </summary>
[TestClass]
public class RenamedPropertyTest : ConversionTestBase<RenamedPropertyComposition>
{
    public RenamedPropertyTest()
        : base(@".\Validations\RenamedProperty\RenamedPropertyComposition.cs")
    {
    }

    [TestMethod]
    public void WhenThePropertyWasRenamedThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    /// <summary>
    /// The target side of the rename is the one that can not be filled, so it is the one named.
    /// </summary>
    [TestMethod]
    public void WhenThePropertyWasRenamedThenTheErrorNamesTheTargetProperty()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.EndsWith(": " + nameof(RenamedPropertyTestCase.To.BB)),
            "The listed properties are expected to be exactly the ones nothing fills, but were: " + message);
    }

    [TestMethod]
    public void WhenThePropertyWasRenamedThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
