namespace Majipro.Converter.Converters;

internal sealed class StringToByteConverter : IConverter<string, byte>
{
    public byte Convert(string from)
    {
        if (byte.TryParse(from, out var i))
        {
            return i;
        }

        return 0;
    }
}