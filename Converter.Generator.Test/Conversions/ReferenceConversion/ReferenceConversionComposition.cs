using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ReferenceConversion;

public class ReferenceConversionComposition : TestCompositionBase
{
    public ReferenceConversionTestCase.To Convert(
        IConvertingService convertingService,
        ReferenceConversionTestCase.From from)
    {
        return convertingService
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(from);
    }

    /// <summary>
    /// The same pair asked for the other way around. It needs an <c>IReferenceConverter</c>, which
    /// is the same generated class - a call site of either shape is the same request.
    /// </summary>
    public ReferenceConversionTestCase.To ConvertToExisting(
        IConvertingService convertingService,
        ReferenceConversionTestCase.From from,
        ReferenceConversionTestCase.To to)
    {
        return convertingService
            .Convert<ReferenceConversionTestCase.From, ReferenceConversionTestCase.To>(from, to);
    }
}
