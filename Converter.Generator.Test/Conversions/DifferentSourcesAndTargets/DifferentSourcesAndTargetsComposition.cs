using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.DifferentSourcesAndTargets;

public class DifferentSourcesAndTargetsComposition : TestCompositionBase
{
    public DifferentSourcesAndTargetsTestCase.FewerProperties ConvertToFewerProperties(
        IConvertingService convertingService,
        DifferentSourcesAndTargetsTestCase.MoreProperties from)
    {
        return convertingService
            .Convert<DifferentSourcesAndTargetsTestCase.MoreProperties, DifferentSourcesAndTargetsTestCase.FewerProperties>(from);
    }

    public DifferentSourcesAndTargetsTestCase.WithoutCompany ConvertToWithoutCompany(
        IConvertingService convertingService,
        DifferentSourcesAndTargetsTestCase.WithCompany from)
    {
        return convertingService
            .Convert<DifferentSourcesAndTargetsTestCase.WithCompany, DifferentSourcesAndTargetsTestCase.WithoutCompany>(from);
    }
}
