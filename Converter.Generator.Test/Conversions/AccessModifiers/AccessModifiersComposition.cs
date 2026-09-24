using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.AccessModifiers;

public class AccessModifiersComposition : TestCompositionBase
{
    public AccessModifiersTestCase.PublicPropertyTo ConvertPublicProperty(
        IConvertingService convertingService,
        AccessModifiersTestCase.PublicPropertyFrom from)
    {
        return convertingService
            .Convert<AccessModifiersTestCase.PublicPropertyFrom, AccessModifiersTestCase.PublicPropertyTo>(from);
    }

    public AccessModifiersTestCase.PrivatePropertyTo ConvertPrivateProperty(
        IConvertingService convertingService,
        AccessModifiersTestCase.PrivatePropertyFrom from)
    {
        return convertingService
            .Convert<AccessModifiersTestCase.PrivatePropertyFrom, AccessModifiersTestCase.PrivatePropertyTo>(from);
    }

    public AccessModifiersTestCase.InitPropertyTo ConvertInitProperty(
        IConvertingService convertingService,
        AccessModifiersTestCase.InitPropertyFrom from)
    {
        return convertingService
            .Convert<AccessModifiersTestCase.InitPropertyFrom, AccessModifiersTestCase.InitPropertyTo>(from);
    }

    public AccessModifiersTestCase.GetOnlyPropertyTo ConvertGetOnlyProperty(
        IConvertingService convertingService,
        AccessModifiersTestCase.GetOnlyPropertyFrom from)
    {
        return convertingService
            .Convert<AccessModifiersTestCase.GetOnlyPropertyFrom, AccessModifiersTestCase.GetOnlyPropertyTo>(from);
    }

    public AccessModifiersTestCase.PrivateSetterTo ConvertPrivateSetter(
        IConvertingService convertingService,
        AccessModifiersTestCase.PrivateSetterFrom from)
    {
        return convertingService
            .Convert<AccessModifiersTestCase.PrivateSetterFrom, AccessModifiersTestCase.PrivateSetterTo>(from);
    }
}
