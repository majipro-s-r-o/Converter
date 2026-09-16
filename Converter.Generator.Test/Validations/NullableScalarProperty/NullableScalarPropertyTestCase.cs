namespace Majipro.Converter.Generator.Test.Validations.NullableScalarProperty;

public class NullableScalarPropertyTestCase
{
    /// <summary>
    /// The text of a scalar is read back by the built-in converters of <c>Converter\Converters</c>,
    /// and those are registered for the value and not for its nullable counterpart. Handing the
    /// value over anyway would mean deciding what an unparseable string makes of an <c>int?</c> -
    /// <c>0</c>, the way the built-in answers, or <c>null</c>, the way the target reads. Nobody has
    /// decided, so the property is refused rather than given an answer picked by the generator.
    /// </summary>
    public class From
    {
        public string Count { get; set; }
    }

    public class To
    {
        public int? Count { get; set; }
    }
}
