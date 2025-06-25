namespace Majipro.Converter.Converters;

internal sealed class StringToIntegerConverter : IConverter<string, int>
{
    public int Convert(string from)
    {
        if (int.TryParse(from, out var i))
        {
            return i;
        }

        return 0;
    }
}