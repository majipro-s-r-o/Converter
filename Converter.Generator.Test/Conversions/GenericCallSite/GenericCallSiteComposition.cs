using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.GenericCallSite;

public class GenericCallSiteComposition : TestCompositionBase
{
    /// <summary>
    /// A call site whose types are only known where it is written. There is no converter to
    /// generate for it, and the pair it asks for is not a name a generated file could carry.
    /// </summary>
    public string ConvertToString<TFrom>(IConvertingService convertingService, TFrom from)
    {
        return convertingService.Convert<TFrom, string>(from);
    }

    public GenericCallSiteTestCase.To Convert(
        IConvertingService convertingService,
        GenericCallSiteTestCase.From from)
    {
        return convertingService.Convert<GenericCallSiteTestCase.From, GenericCallSiteTestCase.To>(from);
    }
}
