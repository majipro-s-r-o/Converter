namespace Majipro.Converter.Generator.Test.Validations.UnusedSourceProperty;

public class UnusedSourcePropertyTestCase
{
    /// <summary>
    /// The rename of <see cref="Validations.RenamedProperty"/> seen from the other side. The source
    /// has a property to spare, but a spare property is not a counterpart: <c>B</c> still has
    /// nothing to come from, and the source having more than the target is no consolation.
    /// </summary>
    public class From
    {
        public string A { get; set; }

        public string BB { get; set; }

        public string C { get; set; }
    }

    public class To
    {
        public string A { get; set; }

        public string B { get; set; }

        public string C { get; set; }
    }
}
