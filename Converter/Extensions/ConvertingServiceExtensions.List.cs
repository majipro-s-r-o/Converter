using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Extensions;

public static partial class ConvertingServiceExtensions
{
    public static List<TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, List<TFrom> from, Func<List<TTo>> nullFallback)
    {
        return convertingService
            .Convert<TFrom, TTo>(from, nullFallback)
            .ToList();
    }
    
    public static List<TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, List<TFrom> from)
    {
        return convertingService
            .Convert<TFrom, TTo>(from)
            .ToList();   
    }
}