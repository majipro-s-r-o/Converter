using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.UnusedSourceProperty;

/// <summary>
/// A conversion the generator refuses: the source carries a <c>BB</c> nobody asked for and the
/// target wants a <c>B</c> nothing fills. It is a compilation of its own, because the error ends
/// the whole generation pass.
/// </summary>
public class UnusedSourcePropertyComposition : TestCompositionBase
{
    public UnusedSourcePropertyTestCase.To ConvertUnusedSourceProperty(
        IConvertingService convertingService,
        UnusedSourcePropertyTestCase.From from)
    {
        return convertingService
            .Convert<UnusedSourcePropertyTestCase.From, UnusedSourcePropertyTestCase.To>(from);
    }
}
