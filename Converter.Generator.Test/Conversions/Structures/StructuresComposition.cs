using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.Structures;

public class StructuresComposition : TestCompositionBase
{
    public StructuresTestCase.ClassWithNullableId ConvertToNullableId(
        IConvertingService convertingService,
        StructuresTestCase.ClassWithId from)
    {
        return convertingService.Convert<StructuresTestCase.ClassWithId, StructuresTestCase.ClassWithNullableId>(from);
    }

    public StructuresTestCase.OtherStructWithId ConvertStructToStruct(
        IConvertingService convertingService,
        StructuresTestCase.StructWithId from)
    {
        return convertingService.Convert<StructuresTestCase.StructWithId, StructuresTestCase.OtherStructWithId>(from);
    }

    public StructuresTestCase.StructTarget ConvertClassToStruct(
        IConvertingService convertingService,
        StructuresTestCase.ClassSource from)
    {
        return convertingService.Convert<StructuresTestCase.ClassSource, StructuresTestCase.StructTarget>(from);
    }

    public StructuresTestCase.ClassTarget ConvertStructToClass(
        IConvertingService convertingService,
        StructuresTestCase.StructSource from)
    {
        return convertingService.Convert<StructuresTestCase.StructSource, StructuresTestCase.ClassTarget>(from);
    }

    public StructuresTestCase.ClassWithStringId ConvertAnnotatedId(
        IConvertingService convertingService,
        StructuresTestCase.RecordWithAnnotatedId from)
    {
        return convertingService
            .Convert<StructuresTestCase.RecordWithAnnotatedId, StructuresTestCase.ClassWithStringId>(from);
    }
}
