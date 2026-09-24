using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.UnconvertibleProperty;

/// <summary>
/// A conversion the generator refuses: <c>To.Status</c> is found by name and then claimed by no
/// value rule. It is a compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class UnconvertiblePropertyComposition : TestCompositionBase
{
    public UnconvertiblePropertyTestCase.To ConvertUnconvertibleProperty(
        IConvertingService convertingService,
        UnconvertiblePropertyTestCase.From from)
    {
        return convertingService
            .Convert<UnconvertiblePropertyTestCase.From, UnconvertiblePropertyTestCase.To>(from);
    }
}
