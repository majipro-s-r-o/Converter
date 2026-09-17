namespace Majipro.Converter.Generator.Test.Validations.MissingProperty;

public class MissingPropertyTestCase
{
    /// <summary>
    /// The target knows a <c>B</c> the source does not, so there is nothing to fill it from and the
    /// converter would return a half built object.
    /// </summary>
    public class From
    {
        public string A { get; set; }

        public string C { get; set; }
    }

    public class To
    {
        public string A { get; set; }

        public string B { get; set; }

        public string C { get; set; }
    }
}
