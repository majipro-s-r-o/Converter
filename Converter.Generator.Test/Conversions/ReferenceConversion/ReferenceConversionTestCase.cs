namespace Majipro.Converter.Generator.Test.Conversions.ReferenceConversion;

public class ReferenceConversionTestCase
{
    public class From
    {
        public string Name { get; set; }

        public Nested Nested { get; set; }
    }

    public class To
    {
        /// <summary>
        /// A field, so the generator does not see it at all. Creating a target leaves it at its
        /// default, filling one that already has it leaves it alone - which is the difference
        /// between the two conversions.
        /// </summary>
        public string Untouched;

        public string Name { get; set; }

        public NestedTarget Nested { get; set; }
    }

    public class Nested
    {
        public int Number { get; set; }
    }

    public class NestedTarget
    {
        public int Number { get; set; }
    }
}
