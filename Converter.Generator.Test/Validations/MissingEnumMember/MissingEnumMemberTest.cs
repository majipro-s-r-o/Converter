using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.MissingEnumMember;

/// <summary>
/// The target enum is missing a member the source has, so the conversion can not be written
/// deterministically and the build has to fail instead of the generator guessing.
/// </summary>
[TestClass]
public class MissingEnumMemberTest : ConversionTestBase<MissingEnumMemberComposition>
{
    public MissingEnumMemberTest()
        : base(@".\Validations\MissingEnumMember\MissingEnumMemberComposition.cs")
    {
    }

    [TestMethod]
    public void WhenAMemberHasNoCounterpartThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0002", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenAMemberHasNoCounterpartThenTheErrorNamesIt()
    {
        var message = Diagnostic.Single().GetMessage();

        StringAssert.Contains(message, nameof(MissingEnumMemberTestCase.Wide.Active));
        StringAssert.Contains(message, typeof(MissingEnumMemberTestCase.Wide).FullName!.Replace("+", "."));
        StringAssert.Contains(message, typeof(MissingEnumMemberTestCase.Narrow).FullName!.Replace("+", "."));
    }

    [TestMethod]
    public void WhenAMemberHasNoCounterpartThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
