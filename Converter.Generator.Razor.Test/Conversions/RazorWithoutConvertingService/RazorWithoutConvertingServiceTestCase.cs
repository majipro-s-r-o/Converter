using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorWithoutConvertingService;

/// <summary>
/// A page converting through a service of the project's own that wraps the converting service, and
/// requires a converter for what it is called with.
/// </summary>
public class RazorWithoutConvertingServiceTestCase
{
    public class Person
    {
        public string Name { get; set; }
    }

    public class WrappedView
    {
        public string Name { get; set; }
    }

    public class PersonMapper
    {
        private readonly IConvertingService _convertingService;

        public PersonMapper(IConvertingService convertingService)
        {
            _convertingService = convertingService;
        }

        [RequiresConverter("TFrom", "TTo")]
        public TTo Map<TFrom, TTo>(TFrom from)
        {
            return _convertingService.Convert<TFrom, TTo>(from);
        }
    }
}
