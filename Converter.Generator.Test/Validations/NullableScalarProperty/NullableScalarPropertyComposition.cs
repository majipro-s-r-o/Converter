using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.NullableScalarProperty;

/// <summary>
/// A conversion the generator refuses: <c>To.Count</c> is an <c>int?</c> and the library has a
/// built-in converter for <c>string -&gt; int</c> only. It is a compilation of its own, because the
/// error ends the whole generation pass.
/// </summary>
public class NullableScalarPropertyComposition : TestCompositionBase
{
    public NullableScalarPropertyTestCase.To ConvertNullableScalarProperty(
        IConvertingService convertingService,
        NullableScalarPropertyTestCase.From from)
    {
        return convertingService
            .Convert<NullableScalarPropertyTestCase.From, NullableScalarPropertyTestCase.To>(from);
    }
}
