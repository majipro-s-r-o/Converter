using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites
{
    /// <summary>
    /// Nothing is converted here: every call site of the case is written in its <c>.razor</c> and
    /// <c>.cshtml</c> files, which the compilation only has as additional files.
    /// </summary>
    public class RazorCallSitesComposition : TestCompositionBase
    {
    }
}
