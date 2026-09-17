using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.MissingProperty;

/// <summary>
/// A conversion the generator refuses: <c>To.B</c> has no counterpart in <c>From</c>. It is a
/// compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class MissingPropertyComposition : TestCompositionBase
{
    public MissingPropertyTestCase.To ConvertMissingProperty(
        IConvertingService convertingService,
        MissingPropertyTestCase.From from)
    {
        return convertingService.Convert<MissingPropertyTestCase.From, MissingPropertyTestCase.To>(from);
    }
}
