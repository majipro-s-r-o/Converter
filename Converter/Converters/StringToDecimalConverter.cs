namespace Majipro.Converter.Converters;

internal sealed class StringToDecimalConverter : IConverter<string, decimal>
{
    private readonly IConverterOptions _converterOptions;

    public StringToDecimalConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }

    public decimal Convert(string from)
    {
        if (decimal.TryParse(from, _converterOptions.NumberStyles, _converterOptions.FormatProvider, out var d))
        {
            return d;
        }

        return 0;
    }
}