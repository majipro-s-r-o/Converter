namespace Majipro.Converter.Generator.Test.Conversions.Enums;

public class EnumsTestCase
{
    public enum Status
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    /// <summary>
    /// The same enum on both sides, and the same enum widened to its nullable counterpart. Both are
    /// conversions the compiler performs on its own, so no converter is asked for.
    /// </summary>
    public class From
    {
        public Status Status { get; set; }

        public Status NullableStatus { get; set; }
    }

    public class To
    {
        public Status Status { get; set; }

        public Status? NullableStatus { get; set; }
    }
}
