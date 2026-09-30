using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.SelfNestedGeneric;

public class SelfNestedGenericComposition : TestCompositionBase
{
    public SelfNestedGenericTestCase.TargetNode<int> ConvertNode(
        IConvertingService convertingService,
        SelfNestedGenericTestCase.SourceNode<int> from)
    {
        return convertingService.Convert<
            SelfNestedGenericTestCase.SourceNode<int>,
            SelfNestedGenericTestCase.TargetNode<int>>(from);
    }
}
