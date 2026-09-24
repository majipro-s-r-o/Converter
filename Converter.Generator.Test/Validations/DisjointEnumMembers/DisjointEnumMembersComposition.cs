using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.DisjointEnumMembers;

/// <summary>
/// Neither enum covers the other, so the conversion is refused for the same reason as in
/// <c>Validations\MissingEnumMember</c> - <c>Left.Closed</c> has nowhere to go. It is a
/// compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class DisjointEnumMembersComposition : TestCompositionBase
{
    public DisjointEnumMembersTestCase.Right ConvertDisjointMembers(
        IConvertingService convertingService,
        DisjointEnumMembersTestCase.Left from)
    {
        return convertingService.Convert<DisjointEnumMembersTestCase.Left, DisjointEnumMembersTestCase.Right>(from);
    }
}
