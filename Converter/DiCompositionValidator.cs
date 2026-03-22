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

            var referenceConverterArgs = new HashSet<ConverterImplementationDescriptor>();

            foreach (var implementedInterface in implementedInterfaces)
            {
                if (implementedInterface.IsGenericType == false || implementedInterface.IsGenericTypeDefinition)
                {
                    continue;
                }

                var genericTypeDefinition = implementedInterface.GetGenericTypeDefinition();

                if (genericTypeDefinition == typeof(IReferenceConverter<,>) ||
                    genericTypeDefinition == typeof(IAsyncReferenceConverter<,>))
                {
                    var args = implementedInterface.GetGenericArguments();
                    referenceConverterArgs.Add(new ConverterImplementationDescriptor(args[0], args[1]));
                }
            }

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

                if ((interfaceType == typeof(IConverter<,>) || interfaceType == typeof(IAsyncConverter<,>)) &&
                    referenceConverterArgs.Contains(new ConverterImplementationDescriptor(genericArguments[0], genericArguments[1])))
                {
                    continue;
                }

                descriptors.Add(new ConverterImplementationDescriptor(genericArguments[0], genericArguments[1]));
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

    private readonly record struct ConverterImplementationDescriptor(Type From, Type To)
    {
        public Type From { get; } = From;
        
        public Type To { get; } = To;
    }
}
