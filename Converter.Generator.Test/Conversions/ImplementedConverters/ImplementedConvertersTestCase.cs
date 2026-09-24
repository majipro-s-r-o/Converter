namespace Majipro.Converter.Generator.Test.Conversions.ImplementedConverters;

public class ImplementedConvertersTestCase
{
    /// <summary>
    /// Somebody wrote the value conversion for this pair and not the reference one.
    /// </summary>
    public class WrittenConverterFrom
    {
        public string Name { get; set; }
    }

    public class WrittenConverterTo
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Somebody wrote the reference conversion for this pair, which is both of them.
    /// </summary>
    public class WrittenReferenceFrom
    {
        public string Name { get; set; }
    }

    public class WrittenReferenceTo
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// Nobody wrote anything, so this is the pair the generator has to itself.
    /// </summary>
    public class GeneratedFrom
    {
        public string Name { get; set; }
    }

    public class GeneratedTo
    {
        public string Name { get; set; }
    }
}
