using System.Collections.Generic;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.AssignablePairs;

public class AssignablePairsComposition : TestCompositionBase
{
    public AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Animal> ConvertCovariant(
        IConvertingService convertingService,
        AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Dog> from)
    {
        return convertingService.Convert<
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Dog>,
            AssignablePairsTestCase.IProducer<AssignablePairsTestCase.Animal>>(from);
    }

    public AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Dog> ConvertContravariant(
        IConvertingService convertingService,
        AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Animal> from)
    {
        return convertingService.Convert<
            AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Animal>,
            AssignablePairsTestCase.IConsumer<AssignablePairsTestCase.Dog>>(from);
    }

    public IEnumerable<AssignablePairsTestCase.Animal> ConvertEnumerable(
        IConvertingService convertingService,
        IEnumerable<AssignablePairsTestCase.Dog> from)
    {
        return convertingService.Convert<
            IEnumerable<AssignablePairsTestCase.Dog>,
            IEnumerable<AssignablePairsTestCase.Animal>>(from);
    }

    /// <summary>
    /// A pair that is assignable as well - a <c>Dog</c> is an <c>Animal</c> - and mappable too. The
    /// object initializer is asked first, so this one is converted rather than handed back.
    /// </summary>
    public AssignablePairsTestCase.Animal ConvertMappable(
        IConvertingService convertingService,
        AssignablePairsTestCase.Dog from)
    {
        return convertingService
            .Convert<AssignablePairsTestCase.Dog, AssignablePairsTestCase.Animal>(from);
    }
}
