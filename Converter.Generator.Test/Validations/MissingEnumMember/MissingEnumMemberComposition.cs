using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.MissingEnumMember;

/// <summary>
/// A conversion the generator refuses: <c>Wide.Active</c> has no counterpart in <c>Narrow</c>.
/// It is a compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class MissingEnumMemberComposition : TestCompositionBase
{
    public MissingEnumMemberTestCase.Narrow ConvertMissingMember(
        IConvertingService convertingService,
        MissingEnumMemberTestCase.Wide from)
    {
        return convertingService.Convert<MissingEnumMemberTestCase.Wide, MissingEnumMemberTestCase.Narrow>(from);
    }
}
