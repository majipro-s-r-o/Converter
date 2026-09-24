using System;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.NullableTypes;

public class NullableTypesComposition : TestCompositionBase
{
    public string ConvertNullableInteger(IConvertingService convertingService, int? from)
    {
        return convertingService.Convert<int?, string>(from);
    }

    public string ConvertNullableGuid(IConvertingService convertingService, Guid? from)
    {
        return convertingService.Convert<Guid?, string>(from);
    }

    public NullableTypesTestCase.NullableSourceTo ConvertNullableSource(
        IConvertingService convertingService,
        NullableTypesTestCase.NullableSourceFrom? from)
    {
        return convertingService
            .Convert<NullableTypesTestCase.NullableSourceFrom?, NullableTypesTestCase.NullableSourceTo>(from);
    }

    public NullableTypesTestCase.NullableBothTo? ConvertNullableBoth(
        IConvertingService convertingService,
        NullableTypesTestCase.NullableBothFrom? from)
    {
        return convertingService
            .Convert<NullableTypesTestCase.NullableBothFrom?, NullableTypesTestCase.NullableBothTo?>(from);
    }

    public NullableTypesTestCase.NullableTargetTo? ConvertNullableTarget(
        IConvertingService convertingService,
        NullableTypesTestCase.NullableTargetFrom from)
    {
        return convertingService
            .Convert<NullableTypesTestCase.NullableTargetFrom, NullableTypesTestCase.NullableTargetTo?>(from);
    }
}
