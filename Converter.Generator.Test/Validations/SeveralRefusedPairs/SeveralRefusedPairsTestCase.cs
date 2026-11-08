namespace Majipro.Converter.Generator.Test.Validations.SeveralRefusedPairs;

public class SeveralRefusedPairsTestCase
{
    /// <summary>The target knows a <c>B</c> the source does not.</summary>
    public class From
    {
        public string A { get; set; }
    }

    public class To
    {
        public string A { get; set; }

        public string B { get; set; }
    }

    /// <summary>The target has no <c>Active</c>.</summary>
    public enum Wide
    {
        Unknown = 0,
        Active = 1
    }

    public enum Narrow
    {
        Unknown = 0
    }

    /// <summary>A pair nothing is wrong with, travelled after the refused ones.</summary>
    public class Person
    {
        public string Name { get; set; }
    }

    public class PersonDto
    {
        public string Name { get; set; }
    }
}
