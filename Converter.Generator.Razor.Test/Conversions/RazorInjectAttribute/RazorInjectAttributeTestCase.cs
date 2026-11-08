namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute;

/// <summary>
/// Components that have the converting service injected by Blazor's <c>[Inject]</c> on a C# property
/// rather than by an <c>@inject</c> - so no Razor file of the project says <c>IConvertingService</c>.
/// </summary>
public class RazorInjectAttributeTestCase
{
    public class Person
    {
        public string Name { get; set; }
    }

    /// <summary>Converted by a page whose code behind file injects the service.</summary>
    public class CodeBehindView
    {
        public string Name { get; set; }
    }

    /// <summary>Converted by a page whose base component injects the service.</summary>
    public class BaseComponentView
    {
        public string Name { get; set; }
    }
}
