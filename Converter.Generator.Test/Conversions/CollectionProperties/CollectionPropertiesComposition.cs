using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.CollectionProperties;

public class CollectionPropertiesComposition : TestCompositionBase
{
    public CollectionPropertiesTestCase.To ConvertCollections(
        IConvertingService convertingService,
        CollectionPropertiesTestCase.From from)
    {
        return convertingService
            .Convert<CollectionPropertiesTestCase.From, CollectionPropertiesTestCase.To>(from);
    }

    public CollectionPropertiesTestCase.SameItemsTo ConvertSameItems(
        IConvertingService convertingService,
        CollectionPropertiesTestCase.SameItemsFrom from)
    {
        return convertingService
            .Convert<CollectionPropertiesTestCase.SameItemsFrom, CollectionPropertiesTestCase.SameItemsTo>(from);
    }
}
