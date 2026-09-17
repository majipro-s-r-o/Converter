using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test.Tests;

namespace Majipro.Converter.Generator.Test.Tests.Enums;

public class EnumsComposition : TestCompositionBase
{
    public string ConvertToString(IConvertingService convertingService, EnumsTestCase.Status from)
    {
        return convertingService.Convert<EnumsTestCase.Status, string>(from);
    }

    public string ConvertNullableToString(IConvertingService convertingService, EnumsTestCase.Status? from)
    {
        return convertingService.Convert<EnumsTestCase.Status?, string>(from);
    }

    public EnumsTestCase.To ConvertProperties(IConvertingService convertingService, EnumsTestCase.From from)
    {
        return convertingService.Convert<EnumsTestCase.From, EnumsTestCase.To>(from);
    }

    public EnumsTestCase.UnmappedTo ConvertBetweenEnumTypes(
        IConvertingService convertingService,
        EnumsTestCase.UnmappedFrom from)
    {
        return convertingService.Convert<EnumsTestCase.UnmappedFrom, EnumsTestCase.UnmappedTo>(from);
    }
}
