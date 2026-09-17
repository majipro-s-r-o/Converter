namespace Majipro.Converter.Generator.Test.Tests.Enums;

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

    /// <summary>
    /// One enum per layer, the shape the consuming projects write converters for by hand. An enum is
    /// neither a class nor a structure to the generator, so no value rule claims the property and it
    /// is dropped without a diagnostic.
    /// </summary>
    public enum StatusEntity
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    public class UnmappedFrom
    {
        public Status Status { get; set; }
    }

    public class UnmappedTo
    {
        public StatusEntity Status { get; set; }
    }
}
