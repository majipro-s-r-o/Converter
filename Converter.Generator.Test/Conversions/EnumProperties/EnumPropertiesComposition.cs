using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.EnumProperties;

public class EnumPropertiesComposition : TestCompositionBase
{
    public EnumPropertiesTestCase.PersonDto ConvertPerson(
        IConvertingService convertingService,
        EnumPropertiesTestCase.Person from)
    {
        return convertingService.Convert<EnumPropertiesTestCase.Person, EnumPropertiesTestCase.PersonDto>(from);
    }
}
