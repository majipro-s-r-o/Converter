using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Extensions;

internal static class TypeSymbolExtensions
{
    /// <summary>
    /// Public, non static, non indexer, non compiler generated instance properties, base types included.
    /// Properties hidden by a more derived declaration are returned only once.
    /// </summary>
    internal static IReadOnlyList<IPropertySymbol> GetInstanceProperties(this ITypeSymbol type)
    {
        var result = new List<IPropertySymbol>();
        var names = new HashSet<string>();

        for (var current = type;
             current != null && current.SpecialType != SpecialType.System_Object;
             current = current.BaseType)
        {
            var properties = current
                .GetMembers()
                .OfType<IPropertySymbol>()
                .Where(p => p.IsStatic == false)
                .Where(p => p.IsIndexer == false)
                .Where(p => p.IsImplicitlyDeclared == false)
                .Where(p => p.DeclaredAccessibility == Accessibility.Public);

            foreach (var property in properties)
            {
                if (names.Add(property.Name))
                {
                    result.Add(property);
                }
            }
        }

        return result;
    }

    internal static bool IsReadable(this IPropertySymbol property)
    {
        return property.GetMethod != null && property.GetMethod.DeclaredAccessibility == Accessibility.Public;
    }

    internal static bool IsWritable(this IPropertySymbol property)
    {
        return property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;
    }

    /// <summary>
    /// Types the generator is able to read from: a plain, non generic class or structure.
    /// </summary>
    internal static bool IsMappableSource(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol named &&
               named.IsGenericType == false &&
               named.SpecialType == SpecialType.None &&
               (named.TypeKind == TypeKind.Class || named.TypeKind == TypeKind.Struct) &&
               named.IsVisibleToGeneratedCode();
    }

    /// <summary>
    /// Types the generator is able to create: an <see cref="IsMappableSource"/> type that
    /// is not abstract and can be created by an object initializer.
    /// </summary>
    internal static bool IsMappableTarget(this ITypeSymbol type)
    {
        return type.IsMappableSource() &&
               type.IsAbstract == false &&
               type is INamedTypeSymbol named &&
               named.HasAccessibleParameterlessConstructor();
    }

    internal static bool HasAccessibleParameterlessConstructor(this INamedTypeSymbol type)
    {
        if (type.TypeKind == TypeKind.Struct)
        {
            return true;
        }

        return type.InstanceConstructors.Any(c =>
            c.Parameters.Length == 0 &&
            (c.DeclaredAccessibility == Accessibility.Public || c.DeclaredAccessibility == Accessibility.Internal));
    }

    /// <summary>
    /// The generated converter lives in the same assembly but in its own namespace, so every
    /// type it touches (including the types it is nested in) has to be at least internal.
    /// </summary>
    internal static bool IsVisibleToGeneratedCode(this ITypeSymbol type)
    {
        for (var current = type as INamedTypeSymbol; current != null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility != Accessibility.Public &&
                current.DeclaredAccessibility != Accessibility.Internal)
            {
                return false;
            }
        }

        return true;
    }

    internal static string ToFullyQualifiedName(this ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }
}
