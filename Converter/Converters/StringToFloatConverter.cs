namespace Majipro.Converter.Converters;

internal sealed class StringToFloatConverter : IConverter<string, float>
{
    private readonly IConverterOptions _converterOptions;

    public StringToFloatConverter(IConverterOptions converterOptions)
    {
        _converterOptions = converterOptions;
    }

    public float Convert(string from)
    {
        if (float.TryParse(from, _converterOptions.NumberStyles, _converterOptions.FormatProvider, out var f))
        {
            return f;
        }

        return 0;
    }
}