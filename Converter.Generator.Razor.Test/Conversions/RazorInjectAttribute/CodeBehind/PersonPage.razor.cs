using Majipro.Converter.Abstractions;
using InjectService = Microsoft.AspNetCore.Components.InjectAttribute;
using static Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.RazorInjectAttributeTestCase;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.CodeBehind
{
    /// <summary>
    /// The code behind file of <c>PersonPage.razor</c>, and the one place the converting service is
    /// injected at - through an alias of <c>[Inject]</c>, so it is the attribute that is recognized
    /// rather than how it is spelled.
    /// </summary>
    public partial class PersonPage
    {
        [InjectService]
        private IConvertingService Converter { get; set; }

        public Person Person { get; set; } = new Person();
    }
}
