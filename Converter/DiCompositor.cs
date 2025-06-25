using System;
using System.Linq;
using System.Reflection;
using Majipro.Converter.Converters;
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
    /// Registers <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/> for assembly where <see cref="T"/> belongs.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The <see cref="Action{ConverterOptions}"/> that allows to configure converters.</param>
    /// <typeparam name="T">Reference <see cref="T"/>.</typeparam>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConverting<T>(this IServiceCollection serviceCollection, Action<ConverterOptions> configureOptions)
    {
        return serviceCollection.AddConverting(configureOptions, typeof(T).Assembly);
    }

    /// <summary>
    /// Registers <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/> for assemblies from <see cref="assemblies"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <param name="assemblies">Array of <see cref="Assembly"/> that will be scanned for <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConverting(this IServiceCollection serviceCollection, params Assembly[] assemblies)
    {
        var options = new ConverterOptions();
        
        return serviceCollection.AddConverting(options, assemblies);
    }
    
    /// <summary>
    /// Registers <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/> for assemblies from <see cref="assemblies"/>.
    /// </summary>
    /// <param name="serviceCollection">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configureOptions">The <see cref="Action{ConverterOptions}"/> that allows to configure converters.</param>
    /// <param name="assemblies">Array of <see cref="Assembly"/> that will be scanned for <see cref="Majipro.Converter.IConverter{TInput,TOutput}"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddConverting(this IServiceCollection serviceCollection, Action<ConverterOptions> configureOptions, params Assembly[] assemblies)
    {
        var options = new ConverterOptions();

        if (configureOptions != null)
        {
            configureOptions(options);
        }

        return serviceCollection.AddConverting(options, assemblies);
    }
    
    private static IServiceCollection AddConverting(this IServiceCollection serviceCollection, ConverterOptions options, params Assembly[] assemblies)
    {
        if (serviceCollection == null)
        {
            throw new ArgumentNullException(nameof(serviceCollection));
        }
        
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }
        
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

            var converterServiceDescriptor = ServiceDescriptor.Describe(iface, converterImplementation, options.ServiceLifetime);
            serviceCollection.TryAdd(converterServiceDescriptor);
        }

        // Add after user converters registration, so the user can override.
        // Build in converters will have fixed lifetime no matter of user settings because it is 
        // not possible to inject any other service there.
        serviceCollection.AddBuildInConverters();

        var optionsServiceDescriptor = ServiceDescriptor.Describe(typeof(IConverterOptions), _ => options, ServiceLifetime.Singleton);
        serviceCollection.Add(optionsServiceDescriptor);

        return serviceCollection;
    }
}