using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.UnusedSourceProperty;

/// <summary>
/// A conversion the generator refuses: the source carries a <c>BB</c> nobody asked for and the
/// target wants a <c>B</c> nothing fills. It is a compilation of its own, so the error it
/// is about is the only one reported.
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
