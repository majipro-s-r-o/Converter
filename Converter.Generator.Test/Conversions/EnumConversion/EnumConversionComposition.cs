using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.EnumConversion;

public class EnumConversionComposition : TestCompositionBase
{
    public EnumConversionTestCase.StatusEntity ConvertSameMembers(
        IConvertingService convertingService,
        EnumConversionTestCase.Status from)
    {
        return convertingService.Convert<EnumConversionTestCase.Status, EnumConversionTestCase.StatusEntity>(from);
    }

    public EnumConversionTestCase.Full ConvertFewerMembers(
        IConvertingService convertingService,
        EnumConversionTestCase.Trimmed from)
    {
        return convertingService.Convert<EnumConversionTestCase.Trimmed, EnumConversionTestCase.Full>(from);
    }

    public EnumConversionTestCase.StatusEntity ConvertFromNullable(
        IConvertingService convertingService,
        EnumConversionTestCase.Status? from)
    {
        return convertingService.Convert<EnumConversionTestCase.Status?, EnumConversionTestCase.StatusEntity>(from);
    }

    public EnumConversionTestCase.StatusEntity? ConvertToNullable(
        IConvertingService convertingService,
        EnumConversionTestCase.Status from)
    {
        return convertingService.Convert<EnumConversionTestCase.Status, EnumConversionTestCase.StatusEntity?>(from);
    }
}
