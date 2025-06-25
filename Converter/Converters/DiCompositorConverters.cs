using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majipro.Converter.Converters;

internal static class DiCompositorConverters
{
    internal static IServiceCollection AddBuildInConverters(this IServiceCollection serviceCollection)
    {
        serviceCollection.TryAddSingleton<IConverter<string, bool>, StringToBoolConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, byte>, StringToByteConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, DateTime>, StringToDateTimeConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, DateTimeOffset>, StringToDateTimeOffsetConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, decimal>, StringToDecimalConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, double>, StringToDoubleConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, float>, StringToFloatConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, int>, StringToIntegerConverter>();
        serviceCollection.TryAddSingleton<IConverter<string, long>, StringToLongConverter>();
        
        return serviceCollection;
    }
}