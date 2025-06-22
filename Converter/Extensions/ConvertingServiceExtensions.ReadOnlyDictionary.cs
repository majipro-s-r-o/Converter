using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Extensions;

public static partial class ConvertingServiceExtensions
{
    public static IReadOnlyDictionary<string, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyDictionary<string, TFrom> from, Func<Dictionary<string, TTo>> nullFallback)
    {
        return (IReadOnlyDictionary<string, TTo>)convertingService
            .Convert<string, TFrom, string, TTo>(from.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), nullFallback);
    }
    
    public static IReadOnlyDictionary<string, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyDictionary<string, TFrom> from)
    {
        return (IReadOnlyDictionary<string, TTo>)convertingService
            .Convert<string, TFrom, string, TTo>(from.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
    }
    
    public static IReadOnlyDictionary<int, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyDictionary<int, TFrom> from, Func<Dictionary<int, TTo>> nullFallback)
    {
        return (IReadOnlyDictionary<int, TTo>)convertingService
            .Convert<int, TFrom, int, TTo>(from.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), nullFallback);
    }
    
    public static IReadOnlyDictionary<Guid, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyDictionary<Guid, TFrom> from, Func<Dictionary<Guid, TTo>> nullFallback)
    {
        return (IReadOnlyDictionary<Guid, TTo>)convertingService
            .Convert<Guid, TFrom, Guid, TTo>(from.ToDictionary(kvp => kvp.Key, kvp => kvp.Value), nullFallback);
    }
}