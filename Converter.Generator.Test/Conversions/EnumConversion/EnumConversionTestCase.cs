namespace Majipro.Converter.Generator.Test.Conversions.EnumConversion;

public class EnumConversionTestCase
{
    /// <summary>
    /// The same members on both sides, the shape a layered application ends up with when it
    /// declares its own enum per layer.
    /// </summary>
    public enum Status
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    public enum StatusEntity
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    /// <summary>
    /// Fewer members than the target has, and deliberately numbered differently: every member of
    /// the source has a counterpart, which is all the generator asks for, and the numbers never
    /// take part in the mapping.
    /// </summary>
    public enum Trimmed
    {
        Unknown = 0,
        Closed = 9
    }

    public enum Full
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }
}
