using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Generator.Test.Conversions.ReferencedConverters;

public class ReferencedConvertersTestCase
{
    public class From
    {
        public string Name { get; set; }
    }

    public class To
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// The converter of the case, and the point of the case is where it lives: a test case file is
    /// compiled into the test assembly only, which the compiled test case references. So this is a
    /// converter somebody wrote in another assembly - the class library holding the converters next
    /// to the application calling <c>Convert</c> - and the generator has to see it there.
    /// </summary>
    public class ReferencedConverter : IConverter<From, To>
    {
        public const string Marker = "referenced converter: ";

        public To Convert(From from)
        {
            return new To
            {
                Name = Marker + from.Name
            };
        }
    }
}
