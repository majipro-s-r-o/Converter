namespace Majipro.Converter.Generator.Test.Validations.RenamedProperty;

public class RenamedPropertyTestCase
{
    /// <summary>
    /// The same property under two names. Matching is by name, so <c>B</c> and <c>BB</c> have
    /// nothing to do with each other and the target is left with a <c>BB</c> nothing fills.
    /// </summary>
    public class From
    {
        public string A { get; set; }

        public string B { get; set; }

        public string C { get; set; }
    }

    public class To
    {
        public string A { get; set; }

        public string BB { get; set; }

        public string C { get; set; }
    }
}
