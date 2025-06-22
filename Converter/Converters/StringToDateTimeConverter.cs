using System;

namespace Majipro.Converter.Converters;

internal sealed class StringToDateTimeConverter : IConverter<string, DateTime>
{
    private readonly IConverterOptions _converterOptions;

    public StringToDateTimeConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }

    public DateTime Convert(string from)
    {
        if (DateTime.TryParse(from, _converterOptions.FormatProvider, _converterOptions.DateTimeStyles, out var dateTime))
        {
            return dateTime;
        }

        return default;
    }
}