using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.DisjointEnumMembers;

/// <summary>
/// Neither enum covers the other. Being a subset in the other direction is no help: what matters
/// is that every source member has somewhere to go.
/// </summary>
[TestClass]
public class DisjointEnumMembersTest : ConversionTestBase<DisjointEnumMembersComposition>
{
    public DisjointEnumMembersTest()
        : base(@".\Validations\DisjointEnumMembers\DisjointEnumMembersComposition.cs")
    {
    }

    [TestMethod]
    public void WhenNeitherEnumCoversTheOtherThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0002", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
        StringAssert.Contains(diagnostic.GetMessage(), nameof(DisjointEnumMembersTestCase.Left.Closed));
    }

    [TestMethod]
    public void WhenNeitherEnumCoversTheOtherThenNoConverterIsGenerated()
    {
        Assert.AreEqual(0, GeneratedSources.Count);
    }
}
