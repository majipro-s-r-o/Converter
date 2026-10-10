using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.GenericCallSiteMissingEnumMember;

/// <summary>
/// A generic call site closed into two enums whose members do not line up. It used to compile and
/// throw at runtime about a missing conversion; it is the build that fails now.
/// </summary>
[TestClass]
public class GenericCallSiteMissingEnumMemberTest : ConversionTestBase<GenericCallSiteMissingEnumMemberComposition>
{
    public GenericCallSiteMissingEnumMemberTest()
        : base(@".\Validations\GenericCallSiteMissingEnumMember\GenericCallSiteMissingEnumMemberComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheClosedPairHasAMemberWithoutCounterpartThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0002", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), nameof(GenericCallSiteMissingEnumMemberTestCase.Source.Barcode));
    }

    [TestMethod]
    public void WhenTheClosedPairHasAMemberWithoutCounterpartThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
