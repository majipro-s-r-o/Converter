namespace Majipro.Converter.Converters;

internal sealed class StringToDoubleConverter : IConverter<string, double>
{
    private readonly IConverterOptions _converterOptions;

    public StringToDoubleConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }

    public double Convert(string from)
    {
        if (double.TryParse(from, _converterOptions.NumberStyles, _converterOptions.FormatProvider, out var d))
        {
            return d;
        }

        return 0;
    }
}