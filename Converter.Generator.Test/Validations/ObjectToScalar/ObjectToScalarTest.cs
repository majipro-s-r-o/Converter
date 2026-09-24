using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.ObjectToScalar;

/// <summary>
/// The other half of <c>MC0004</c>: the source is perfectly mappable and the target is not
/// something an object initializer creates, so the pair falls through every body rule. A target the
/// generator can not build is a refusal, not a silent skip.
/// </summary>
[TestClass]
public class ObjectToScalarTest : ConversionTestBase<ObjectToScalarComposition>
{
    public ObjectToScalarTest()
        : base(@".\Validations\ObjectToScalar\ObjectToScalarComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheTargetIsNotMappableThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0004", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenTheTargetIsNotMappableThenTheErrorNamesThePair()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.Contains(nameof(ObjectToScalarTestCase.From)) && message.Contains("'int'"),
            "The error is expected to name both sides of the pair, but was: " + message);
    }

    [TestMethod]
    public void WhenTheTargetIsNotMappableThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
