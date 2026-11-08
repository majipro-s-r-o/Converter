namespace Majipro.Converter.Generator.Test.Conversions.RazorGeneratorUnavailable;

public class RazorGeneratorUnavailableTestCase
{
    public class Person
    {
        public string Name { get; set; }
    }

    /// <summary>Converted in the composition, which is C# - generated whatever becomes of the Razor files.</summary>
    public class PersonView
    {
        public string Name { get; set; }
    }

    /// <summary>Converted only in a Razor file, which nothing compiles here.</summary>
    public class RazorView
    {
        public string Name { get; set; }
    }
}
