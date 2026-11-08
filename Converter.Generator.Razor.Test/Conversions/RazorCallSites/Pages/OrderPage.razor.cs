using System.Collections.Generic;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites.RazorCallSitesTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites.Pages
{
    /// <summary>
    /// The code behind file of <c>Pages\OrderPage.razor</c>, which <c>@inject</c>s the converting
    /// service itself rather than through <c>_Imports.razor</c>: <c>Mapper</c> is declared by that one
    /// line of the component and nowhere else, and this file converts through it.
    /// </summary>
    public partial class OrderPage
    {
        public Person Buyer { get; set; }

        public List<Person> People { get; set; }

        private InjectedCodeBehindView ToView()
        {
            return Mapper.Convert<Person, InjectedCodeBehindView>(Buyer);
        }
    }
}
