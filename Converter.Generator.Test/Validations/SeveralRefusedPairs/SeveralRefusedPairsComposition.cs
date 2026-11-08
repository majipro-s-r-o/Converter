using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.SeveralRefusedPairs;

/// <summary>
/// Three pairs the generator refuses, each for a reason of its own, and one it writes. A refused
/// pair fails the build without ending the generation, so all three are reported by one build.
/// </summary>
public class SeveralRefusedPairsComposition : TestCompositionBase
{
    public SeveralRefusedPairsTestCase.To ConvertMissingProperty(
        IConvertingService convertingService,
        SeveralRefusedPairsTestCase.From from)
    {
        return convertingService.Convert<SeveralRefusedPairsTestCase.From, SeveralRefusedPairsTestCase.To>(from);
    }

    public SeveralRefusedPairsTestCase.Narrow ConvertMissingEnumMember(
        IConvertingService convertingService,
        SeveralRefusedPairsTestCase.Wide from)
    {
        return convertingService.Convert<SeveralRefusedPairsTestCase.Wide, SeveralRefusedPairsTestCase.Narrow>(from);
    }

    public float ConvertNumber(IConvertingService convertingService, double from)
    {
        return convertingService.Convert<double, float>(from);
    }

    public SeveralRefusedPairsTestCase.PersonDto ConvertPerson(
        IConvertingService convertingService,
        SeveralRefusedPairsTestCase.Person from)
    {
        return convertingService.Convert<SeveralRefusedPairsTestCase.Person, SeveralRefusedPairsTestCase.PersonDto>(from);
    }
}
