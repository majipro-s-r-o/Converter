using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ObjectNesting;

public class ObjectNestingComposition : TestCompositionBase
{
    public ObjectNestingTestCase.To ConvertNested(
        IConvertingService convertingService,
        ObjectNestingTestCase.From from)
    {
        return convertingService.Convert<ObjectNestingTestCase.From, ObjectNestingTestCase.To>(from);
    }

    public ObjectNestingTestCase.CollectionTo ConvertCollection(
        IConvertingService convertingService,
        ObjectNestingTestCase.CollectionFrom from)
    {
        return convertingService
            .Convert<ObjectNestingTestCase.CollectionFrom, ObjectNestingTestCase.CollectionTo>(from);
    }
}
