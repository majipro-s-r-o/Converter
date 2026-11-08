using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Test.Validations.EnumPropertyMissingMember;

/// <summary>
/// An enum property whose enums do not line up is <c>MC0002</c> naming the member, the same as the
/// pair at a call site - not <c>MC0003</c>, which would only say the property is unfilled.
/// </summary>
[TestClass]
public class EnumPropertyMissingMemberTest : ConversionTestBase<EnumPropertyMissingMemberComposition>
{
    public EnumPropertyMissingMemberTest()
        : base(@".\Validations\EnumPropertyMissingMember\EnumPropertyMissingMemberComposition.cs")
    {
    }

    [TestMethod]
    public void WhenTheEnumPropertyHasAMemberWithoutCounterpartThenAnErrorIsReported()
    {
        var diagnostic = Diagnostic.Single();

        Assert.AreEqual("MC0002", diagnostic.Id);
        Assert.AreEqual(DiagnosticSeverity.Error, diagnostic.Severity);
    }

    [TestMethod]
    public void WhenTheEnumPropertyHasAMemberWithoutCounterpartThenTheErrorNamesIt()
    {
        StringAssert.Contains(
            Diagnostic.Single().GetMessage(),
            nameof(EnumPropertyMissingMemberTestCase.Wide.Active));
    }
}
