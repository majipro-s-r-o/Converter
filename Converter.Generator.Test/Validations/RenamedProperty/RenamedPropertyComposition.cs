using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.RenamedProperty;

/// <summary>
/// A conversion the generator refuses: the source carries a <c>B</c> and the target wants a
/// <c>BB</c>. It is a compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class RenamedPropertyComposition : TestCompositionBase
{
    public RenamedPropertyTestCase.To ConvertRenamedProperty(
        IConvertingService convertingService,
        RenamedPropertyTestCase.From from)
    {
        return convertingService.Convert<RenamedPropertyTestCase.From, RenamedPropertyTestCase.To>(from);
    }
}
