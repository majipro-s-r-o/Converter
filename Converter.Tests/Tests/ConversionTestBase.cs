using System;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Tests.ConvertingService;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Majipro.Converter.Tests.Tests;

public abstract class ConversionTestBase
{
    protected static IConvertingService ConvertingService;

    protected static async Task ClassInitializeAsync()
    {
        await ClassInitializeAsync(_ => { });
    }
    
    protected static async Task ClassInitializeAsync(Action<ConverterOptions> configureOptions)
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                services.AddConverting<ListConversionTest>(configureOptions);
            })
            .Build();

        ConvertingService = host.Services.GetRequiredService<IConvertingService>();
        
        await host.StartAsync();
    }
}