using System.Linq;
using System.Reflection;
using Majipro.Converter.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Majipro.Converter;

public static class DiCompositor
{
    /// <summary>
    /// Registers <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/> for assembly where <see cref="T"/> belongs.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <typeparam name="T">Reference <see cref="T"/>.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConverting<T>(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddConverting(typeof(T).Assembly);
    }
    
    /// <summary>
    /// Registers <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/> for assemblies from <see cref="assemblies"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <param name="assemblies">Array of <see cref="Assembly"/> that will be scanned for <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConverting(this IServiceCollection serviceCollection, params Assembly[] assemblies)
    {
        serviceCollection.TryAddSingleton<IConvertingService, ConvertingService>();

        var converterImplementations = assemblies
            .SelectMany(s => s.GetTypes())
            .Where(t => t.IsAssignable(typeof(IConverter<,>)) || t.IsAssignable(typeof(IReferenceConverter<,>)));

        foreach (var converterImplementation in converterImplementations)
        {
            var iface = converterImplementation
                .GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IConverter<,>) || i.GetGenericTypeDefinition() == typeof(IReferenceConverter<,>));

            if (iface == null)
            {
                continue;
            }

            serviceCollection.TryAddSingleton(iface, converterImplementation);
        }

        return serviceCollection;
    }
}