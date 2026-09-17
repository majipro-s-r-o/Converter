using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.MissingProperty;

/// <summary>
/// The target declares a property the source does not have. Filling it is impossible and leaving
/// it at its default would be a converter that silently loses a part of the target, so the build
/// has to fail instead.
/// </summary>
[TestClass]
public class MissingPropertyTest : ConversionTestBase<MissingPropertyComposition>
{
    public MissingPropertyTest()
        : base(@".\Validations\MissingProperty\MissingPropertyComposition.cs")
    {
    }

    [TestMethod]
    public void WhenAPropertyHasNoCounterpartThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenAPropertyHasNoCounterpartThenTheErrorNamesIt()
    {
        var message = Diagnostic.Single().GetMessage();

        StringAssert.Contains(message, nameof(MissingPropertyTestCase.To.B));
        StringAssert.Contains(message, typeof(MissingPropertyTestCase.From).FullName!.Replace("+", "."));
        StringAssert.Contains(message, typeof(MissingPropertyTestCase.To).FullName!.Replace("+", "."));
    }

    /// <summary>
    /// The error is about the properties that can not be filled, so the ones that do line up are
    /// not in the list it ends with.
    /// </summary>
    [TestMethod]
    public void WhenAPropertyHasNoCounterpartThenTheMappedOnesAreNotNamed()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.EndsWith(": " + nameof(MissingPropertyTestCase.To.B)),
            "The listed properties are expected to be exactly the ones nothing fills, but were: " + message);
    }

    [TestMethod]
    public void WhenAPropertyHasNoCounterpartThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
