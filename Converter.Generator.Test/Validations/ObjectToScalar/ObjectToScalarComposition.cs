using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.ObjectToScalar;

/// <summary>
/// A conversion the generator refuses: a class into a scalar. The source is a type the generator
/// reads, the target is not a type it builds, and no rule claims the pair. It is a compilation of
/// its own, so the error it is about is the only one reported.
/// </summary>
public class ObjectToScalarComposition : TestCompositionBase
{
    public int ConvertObjectToScalar(IConvertingService convertingService, ObjectToScalarTestCase.From from)
    {
        return convertingService.Convert<ObjectToScalarTestCase.From, int>(from);
    }
}
