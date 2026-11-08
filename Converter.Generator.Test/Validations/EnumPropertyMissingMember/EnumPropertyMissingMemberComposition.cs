using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.EnumPropertyMissingMember;

/// <summary>
/// A conversion the generator refuses: the property hands its enum pair on, and the pair has a
/// member without a counterpart. It is a compilation of its own, because the error ends the whole
/// generation pass.
/// </summary>
public class EnumPropertyMissingMemberComposition : TestCompositionBase
{
    public EnumPropertyMissingMemberTestCase.To ConvertEnumProperty(
        IConvertingService convertingService,
        EnumPropertyMissingMemberTestCase.From from)
    {
        return convertingService
            .Convert<EnumPropertyMissingMemberTestCase.From, EnumPropertyMissingMemberTestCase.To>(from);
    }
}
