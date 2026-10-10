using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.GenericCallSiteMissingEnumMember;

/// <summary>
/// A generic call site closed into a pair the generator refuses. The pair has to fail the build the
/// same way it would written out - otherwise a generic helper is a way around every diagnostic.
/// </summary>
public class GenericCallSiteMissingEnumMemberComposition : TestCompositionBase
{
    public GenericCallSiteMissingEnumMemberTestCase.Target ConvertMissingMember(
        IConvertingService convertingService,
        GenericCallSiteMissingEnumMemberTestCase.Source from)
    {
        return ConvertKind(convertingService, from);
    }

    private GenericCallSiteMissingEnumMemberTestCase.Target ConvertKind<TKind>(
        IConvertingService convertingService,
        TKind kind)
    {
        return convertingService.Convert<TKind, GenericCallSiteMissingEnumMemberTestCase.Target>(kind);
    }
}
