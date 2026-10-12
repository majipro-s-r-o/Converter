using System;
using System.Threading.Tasks;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Tests.Tests.ConvertingService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Majipro.Converter.Tests.Tests;

public abstract class ConversionTestBase
{
    protected static IConvertingService ConvertingService;

    /// <summary>
    /// The converting service every test of the class shares, with the default options. Set once per
    /// class from <c>[ClassInitialize]</c>; classes run in parallel, so nothing may write it later.
    /// </summary>
    protected static async Task ClassInitializeAsync()
    {
        ConvertingService = await CreateConvertingServiceAsync(_ => { });
    }

    /// <summary>
    /// A converting service of the test's own, for a test that needs options of its own. It must not
    /// be assigned to <see cref="ConvertingService"/>: that field is shared by every class, they run in
    /// parallel, and a test reconfiguring it hands its options to whichever other test reads it next -
    /// which is how <c>"$ 123,456.78"</c> used to be parsed with another class's <c>cs-CZ</c>.
    /// </summary>
    protected static async Task<IConvertingService> CreateConvertingServiceAsync(Action<ConverterOptions> configureOptions)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddConverting<ListConversionTest>(configureOptions);
            })
            .Build();

        await host.StartAsync();

        return host.Services.GetRequiredService<IConvertingService>();
    }
}