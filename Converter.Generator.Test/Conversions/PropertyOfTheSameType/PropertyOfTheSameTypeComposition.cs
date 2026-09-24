using Majipro.Converter.Abstrations;
using Majipro.Converter;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.PropertyOfTheSameType;

public class PropertyOfTheSameTypeComposition : TestCompositionBase
{
    /// <summary>
    /// The conversion request. This call site is what the generator reacts on, it does not have to
    /// be called by anybody to make the converter appear.
    /// </summary>
    public PropertyOfTheSameTypeTestCase.To Convert(
        IConvertingService convertingService,
        PropertyOfTheSameTypeTestCase.From from)
    {
        return convertingService.Convert<PropertyOfTheSameTypeTestCase.From, PropertyOfTheSameTypeTestCase.To>(from);
    }
}
