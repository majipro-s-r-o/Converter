using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter;

public interface IConverterOptions
{
    public ServiceLifetime ServiceLifetime { get; }
    
    public IFormatProvider FormatProvider { get; }

    public NumberStyles NumberStyles { get; }
    
    DateTimeStyles DateTimeStyles { get; }
}