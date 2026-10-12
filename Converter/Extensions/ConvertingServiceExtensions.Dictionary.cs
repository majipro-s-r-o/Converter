using System;
using System.Collections.Generic;
using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Extensions;

public static partial class ConvertingServiceExtensions
{
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<string, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<string, TFrom> from, Func<Dictionary<string, TTo>> nullFallback)
    {
        return (Dictionary<string, TTo>)convertingService
            .Convert<string, TFrom, string, TTo>(from, nullFallback);
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<string, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<string, TFrom> from)
    {
        return (Dictionary<string, TTo>)convertingService
            .Convert<string, TFrom, string, TTo>(from);
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<int, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<int, TFrom> from, Func<Dictionary<int, TTo>> nullFallback)
    {
        return (Dictionary<int, TTo>)convertingService
            .Convert<int, TFrom, int, TTo>(from, nullFallback);
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<int, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<int, TFrom> from)
    {
        return (Dictionary<int, TTo>)convertingService
            .Convert<int, TFrom, int, TTo>(from);
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<Guid, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<Guid, TFrom> from, Func<Dictionary<Guid, TTo>> nullFallback)
    {
        return (Dictionary<Guid, TTo>)convertingService
            .Convert<Guid, TFrom, Guid, TTo>(from, nullFallback);
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static Dictionary<Guid, TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, Dictionary<Guid, TFrom> from)
    {
        return (Dictionary<Guid, TTo>)convertingService
            .Convert<Guid, TFrom, Guid, TTo>(from);
    }
}