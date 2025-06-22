using System;

namespace Majipro.Converter.Converters;

internal sealed class StringToDateTimeOffsetConverter : IConverter<string, DateTimeOffset>
{
    private readonly IConverterOptions _converterOptions;

    public StringToDateTimeOffsetConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }

    public DateTimeOffset Convert(string from)
    {
        if (DateTimeOffset.TryParse(from, _converterOptions.FormatProvider, _converterOptions.DateTimeStyles, out var dateTimeOffset))
        {
            return dateTimeOffset;
        }

        return default;
    }
}