using Majipro.Converter.Abstractions;
using Microsoft.AspNetCore.Components;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorInjectAttribute.BaseComponent
{
    /// <summary>A base component of the project's own, the one place the converting service is injected at.</summary>
    public abstract class ConvertingComponentBase : ComponentBase
    {
        [Inject]
        protected IConvertingService Converter { get; set; }
    }
}
