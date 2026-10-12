using System;
using System.Linq;
using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Extensions;

public static partial class ConvertingServiceExtensions
{
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static TTo[] ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, TFrom[] from, Func<TTo[]> nullFallback)
    {
        return convertingService
            .Convert<TFrom, TTo>(from, nullFallback)
            .ToArray();   
    }
    
    [RequiresConverter(nameof(TFrom), nameof(TTo))]
    public static TTo[] ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, TFrom[] from)
    {
        return convertingService
            .Convert<TFrom, TTo>(from)
            .ToArray();   
    }
}