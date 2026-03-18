using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter;

internal static class DiCompositionValidator
{
    internal static void ValidateOrThrow(IList<Type> converterImplementationsTypes)
    {
        var descriptors = new List<ConverterImplementationDescriptor>();
        
        foreach (var converterImplementationType in converterImplementationsTypes)
        {
            var implementedInterfaces = converterImplementationType.GetInterfaces();

            foreach (var implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.IsGenericTypeDefinition)
                {
                    continue;    
                }
                
                var interfaceType = implementedInterface.GetGenericTypeDefinition();

                if (interfaceType != typeof(IConverter<,>) &&
                    interfaceType != typeof(IAsyncConverter<,>) &&
                    interfaceType != typeof(IReferenceConverter<,>) &&
                    interfaceType != typeof(IAsyncReferenceConverter<,>))
                {
                    continue;
                }
                
                var genericArguments = implementedInterface.GetGenericArguments();

                var converterImplementationDescriptor = new ConverterImplementationDescriptor(genericArguments);
                
                descriptors.Add(converterImplementationDescriptor);
            }
        }
        
        var duplicates = descriptors
            .GroupBy(d => d)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicates.Count > 0)
        {
            var messages = duplicates
                .Select(g => $"Duplicate converter registration for '{g.Key.From.FullName}' -> '{g.Key.To.FullName}' ('{g.Count()}' registrations)");

            throw new InvalidOperationException($"Duplicate converter registrations found: {string.Join("; ", messages)}");
        }
    }

    private sealed record ConverterImplementationDescriptor
    {
        public Type From { get; }
        
        public Type To { get; }

        public ConverterImplementationDescriptor(params Type[] types)
        {
            From = types[0];
            To = types[1];
        }
    }
}
