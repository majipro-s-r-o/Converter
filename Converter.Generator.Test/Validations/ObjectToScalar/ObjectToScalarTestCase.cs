namespace Majipro.Converter.Generator.Test.Validations.ObjectToScalar;

public class ObjectToScalarTestCase
{
    /// <summary>
    /// A class the generator can read, asked to become a number. Which of its properties the number
    /// would be made of is not something the generator is in a position to decide, and no rule
    /// claims the pair, so the conversion is refused rather than guessed.
    /// </summary>
    public class From
    {
        public int Count { get; set; }
    }
}
