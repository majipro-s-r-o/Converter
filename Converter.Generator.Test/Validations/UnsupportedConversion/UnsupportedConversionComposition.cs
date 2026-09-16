using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.UnsupportedConversion;

/// <summary>
/// A conversion the generator refuses: two numbers. No rule knows how to write the pair, the
/// library has no built-in converter for it, and nobody wrote one - so the call would compile and
/// throw at runtime. It is a compilation of its own, because the error ends the whole generation
/// pass.
/// </summary>
/// <remarks>
/// The pair has to be one no converter in this test assembly implements: the compilation references
/// the test assembly, so a converter written in any other test case would answer this one too, the
/// same way a converter in a class library answers a call site in an application.
/// </remarks>
public class UnsupportedConversionComposition : TestCompositionBase
{
    public float ConvertNumber(IConvertingService convertingService, double from)
    {
        return convertingService.Convert<double, float>(from);
    }
}
