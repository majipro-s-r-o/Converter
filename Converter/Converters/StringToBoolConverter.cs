namespace Majipro.Converter.Converters;

internal sealed class StringToBoolConverter : IConverter<string, bool>
{
    public bool Convert(string from)
    {
        if (bool.TryParse(from?.Trim(), out var b))
        {
            return b;
        }

        if (from?.Trim() == "1")
        {
            return true;
        }
        
        return false;
    }
}