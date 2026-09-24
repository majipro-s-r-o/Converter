namespace Majipro.Converter.Generator.Test.Validations.UnconvertibleProperty;

public class UnconvertiblePropertyTestCase
{
    /// <summary>
    /// One enum per layer, the shape the consuming projects write converters for by hand. The names
    /// line up, so this is not a property that is missing - it is one no value rule knows what to do
    /// with, because an enum is neither a class nor a structure to the generator. The outcome is the
    /// same either way, a target property nothing fills.
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

    public class From
    {
        public Status Status { get; set; }
    }

    public class To
    {
        public StatusEntity Status { get; set; }
    }
}
