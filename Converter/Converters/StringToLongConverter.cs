namespace Majipro.Converter.Converters;

internal sealed class StringToLongConverter : IConverter<string, long>
{
    public long Convert(string from)
    {
        if (long.TryParse(from, out var i))
        {
            return i;
        }

        return 0;
    }
}