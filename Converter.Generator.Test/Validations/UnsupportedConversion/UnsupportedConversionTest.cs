using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.UnsupportedConversion;

/// <summary>
/// The pair nothing answers. A <c>double</c> and a <c>float</c> are two numbers, which is exactly
/// why it is easy to ask the converting service to convert one into the other - and the service
/// knows no such converter, so the call throws at runtime. The generator says so at build time
/// instead.
/// </summary>
[TestClass]
public class UnsupportedConversionTest : ConversionTestBase<UnsupportedConversionComposition>
{
    public UnsupportedConversionTest()
        : base(@".\Validations\UnsupportedConversion\UnsupportedConversionComposition.cs")
    {
    }

    [TestMethod]
    public void WhenNoConverterCanBeWrittenThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0004", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenNoConverterCanBeWrittenThenTheErrorNamesThePair()
    {
        var message = Diagnostic.Single().GetMessage();

        Assert.IsTrue(
            message.Contains("'double'") && message.Contains("'float'"),
            "The error is expected to name both sides of the pair, but was: " + message);
    }

    [TestMethod]
    public void WhenNoConverterCanBeWrittenThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
