using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Extensions;

public static partial class ConvertingServiceExtensions
{
    public static IReadOnlyList<TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyList<TFrom> from, Func<IReadOnlyList<TTo>> nullFallback)
    {
        return convertingService
            .Convert<TFrom, TTo>(from, nullFallback)
            .ToList();
    }
    
    public static IReadOnlyList<TTo> ConvertExplicitly<TFrom, TTo>(this IConvertingService convertingService, IReadOnlyList<TFrom> from)
    {
        return convertingService
            .Convert<TFrom, TTo>(from)
            .ToList();   
    }
}