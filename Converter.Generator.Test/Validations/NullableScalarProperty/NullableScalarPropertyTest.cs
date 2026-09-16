using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.NullableScalarProperty;

/// <summary>
/// The boundary of <c>StringValueRule</c>: it claims a string being read back only for the types
/// the library really registers a converter for, and <c>int?</c> is not one of them. This is the
/// case that keeps that boundary a decision instead of an oversight - widening the rule to nullable
/// targets means saying first what a string that is not a number makes of one.
/// </summary>
[TestClass]
public class NullableScalarPropertyTest : ConversionTestBase<NullableScalarPropertyComposition>
{
    public NullableScalarPropertyTest()
        : base(@".\Validations\NullableScalarProperty\NullableScalarPropertyComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheScalarTargetIsNullableThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0003", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenTheScalarTargetIsNullableThenTheErrorNamesTheProperty()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.EndsWith(": " + nameof(NullableScalarPropertyTestCase.To.Count)),
            "The listed properties are expected to be exactly the ones nothing fills, but were: " + message);
    }

    [TestMethod]
    public void WhenTheScalarTargetIsNullableThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
