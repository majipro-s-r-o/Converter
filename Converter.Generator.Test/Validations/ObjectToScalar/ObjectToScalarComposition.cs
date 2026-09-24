using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.ObjectToScalar;

/// <summary>
/// A conversion the generator refuses: a class into a scalar. The source is a type the generator
/// reads, the target is not a type it builds, and no rule claims the pair. It is a compilation of
/// its own, because the error ends the whole generation pass.
/// </summary>
public class ObjectToScalarComposition : TestCompositionBase
{
    public int ConvertObjectToScalar(IConvertingService convertingService, ObjectToScalarTestCase.From from)
    {
        return convertingService.Convert<ObjectToScalarTestCase.From, int>(from);
    }
}
