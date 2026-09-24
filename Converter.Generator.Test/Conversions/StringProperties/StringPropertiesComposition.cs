using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.StringProperties;

public class StringPropertiesComposition : TestCompositionBase
{
    public StringPropertiesTestCase.TextTo ConvertToText(
        IConvertingService convertingService,
        StringPropertiesTestCase.ScalarFrom from)
    {
        return convertingService
            .Convert<StringPropertiesTestCase.ScalarFrom, StringPropertiesTestCase.TextTo>(from);
    }

    public StringPropertiesTestCase.ScalarTo ConvertFromText(
        IConvertingService convertingService,
        StringPropertiesTestCase.TextFrom from)
    {
        return convertingService
            .Convert<StringPropertiesTestCase.TextFrom, StringPropertiesTestCase.ScalarTo>(from);
    }
}
