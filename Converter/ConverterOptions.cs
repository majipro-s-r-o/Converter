using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter;

/// <summary>
/// Converter configuration object
/// </summary>
public sealed class ConverterOptions : IConverterOptions
{
    /// <summary>
    /// Default <see cref="ServiceLifetime"/> of <see cref="IConverter{TFrom,TTo}"/> services, default <see cref="ServiceLifetime.Singleton"/>.
    /// </summary>
    public ServiceLifetime ServiceLifetime { get; set; } = ServiceLifetime.Singleton;

    /// <summary>
    /// Default <see cref="IFormatProvider"/> for <see cref="IConverter{TFrom,TTo}"/> services, default <see cref="CultureInfo.InvariantCulture"/>.
    /// </summary>
    public IFormatProvider FormatProvider { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Default <see cref="NumberStyles"/> for <see cref="IConverter{TFrom,TTo}"/> services, default <see cref="NumberStyles.Any"/>.
    /// </summary>
    public NumberStyles NumberStyles { get; set; } = NumberStyles.Any;
    
    /// <summary>
    /// Default <see cref="DateTimeStyles"/> for <see cref="IConverter{TFrom,TTo}"/> services, default <see cref="DateTimeStyles.None"/>.
    /// </summary>
    public DateTimeStyles DateTimeStyles { get; set; } = DateTimeStyles.None;
}