using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ReferencedConverters;

/// <summary>
/// The call site alone. The converter for the pair is implemented in a referenced assembly, not
/// here, which is the ordinary shape of a solution: the converters in a class library and the code
/// asking for them in an application.
/// </summary>
public class ReferencedConvertersComposition : TestCompositionBase
{
    public ReferencedConvertersTestCase.To ConvertReferenced(
        IConvertingService convertingService,
        ReferencedConvertersTestCase.From from)
    {
        return convertingService
            .Convert<ReferencedConvertersTestCase.From, ReferencedConvertersTestCase.To>(from);
    }
}
