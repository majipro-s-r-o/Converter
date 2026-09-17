using Majipro.Converter;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter.Generator.Test;

/// <summary>
/// Composition root of a test case. It is compiled together with the test case source files, so
/// <c>GetType().Assembly</c> is the assembly the generator wrote the converters into.
/// </summary>
public abstract class TestCompositionBase
{
    public ServiceProvider Main()
    {
        var serviceCollection = new ServiceCollection();

        serviceCollection.AddConverting(GetType().Assembly);

        return serviceCollection.BuildServiceProvider();
    }
}
