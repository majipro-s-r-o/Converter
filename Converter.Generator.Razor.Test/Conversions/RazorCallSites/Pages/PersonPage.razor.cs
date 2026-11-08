using System.Collections.Generic;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites.RazorCallSitesTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites.Pages
{
    /// <summary>
    /// The code behind file of <c>Pages\PersonPage.razor</c>: the same class, in the namespace the
    /// root namespace and the folder make. The page converts its members without writing a type
    /// argument, so they have to be in scope for the pair to be known at all - and this file converts
    /// through <c>Converter</c>, which only the <c>@inject</c> in <c>_Imports.razor</c> declares. Like a
    /// real code behind file it does not compile without the Razor side, which is why the csproj keeps
    /// it out of the test assembly and the test hands it to the compilation as source code.
    /// </summary>
    public partial class PersonPage
    {
        public Person Person { get; set; }

        public List<Person> People { get; set; }

        private CodeBehindView ToView()
        {
            return Converter.Convert<Person, CodeBehindView>(Person);
        }
    }
}
