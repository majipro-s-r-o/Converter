using Majipro.Converter.Abstractions;
using Majipro.Converter.Generator.Test;
using static Majipro.Converter.Generator.Test.Conversions.RazorGeneratorUnavailable.RazorGeneratorUnavailableTestCase;

namespace Majipro.Converter.Generator.Test.Conversions.RazorGeneratorUnavailable
{
    public class RazorGeneratorUnavailableComposition : TestCompositionBase
    {
        public void Convert(IConvertingService convertingService)
        {
            convertingService.Convert<Person, PersonView>(new Person());
        }
    }
}
