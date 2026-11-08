namespace Majipro.Converter.Generator.Test.Validations.UnconvertibleProperty;

public class UnconvertiblePropertyTestCase
{
    /// <summary>
    /// The names line up, so this is not a property that is missing - it is one no value rule knows
    /// what to do with: a narrowing numeric conversion is not implicit, neither side is mappable, and
    /// the library has no converter between the two. The outcome is the same either way, a target
    /// property nothing fills.
    /// </summary>
    public class From
    {
        public double Amount { get; set; }
    }

    public class To
    {
        public float Amount { get; set; }
    }
}
