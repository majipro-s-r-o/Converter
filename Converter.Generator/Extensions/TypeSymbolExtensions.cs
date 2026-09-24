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

    /// <summary>
    /// The members an enum declares, in declaration order. They are the constant fields of the
    /// type, which is also what tells them from the instance field carrying the value.
    /// </summary>
    internal static IReadOnlyList<IFieldSymbol> GetEnumMembers(this ITypeSymbol type)
    {
        return type
            .GetMembers()
            .OfType<IFieldSymbol>()
            .Where(f => f.IsConst)
            .ToList();
    }

    internal static bool IsReadable(this IPropertySymbol property)
    {
        return property.GetMethod != null && property.GetMethod.DeclaredAccessibility == Accessibility.Public;
    }

    /// <summary>
    /// Writable by an object initializer, so an <c>init</c> only setter counts as well.
    /// </summary>
    internal static bool IsWritable(this IPropertySymbol property)
    {
        return property.SetMethod != null && property.SetMethod.DeclaredAccessibility == Accessibility.Public;
    }

    /// <summary>
    /// Writable on an instance that already exists, which an <c>init</c> only setter is not: it can
    /// be written while the object is being created and never again. That is the difference between
    /// a target a converter can create and a target a reference converter can fill.
    /// </summary>
    internal static bool IsSettable(this IPropertySymbol property)
    {
        return property.IsWritable() && property.SetMethod!.IsInitOnly == false;
    }

    /// <summary>
    /// <c>T</c> of a <see cref="System.Nullable{T}"/>, <c>null</c> for anything else.
    /// </summary>
    internal static ITypeSymbol? GetNullableUnderlyingType(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol named &&
               named.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T &&
               named.TypeArguments.Length == 1
            ? named.TypeArguments[0]
            : null;
    }

    /// <summary>
    /// The type itself, or <c>T</c> when the type is a <see cref="System.Nullable{T}"/>. This is the
    /// type whose properties are mapped, while the nullable type is what the converter signature says.
    /// </summary>
    internal static ITypeSymbol GetUnderlyingType(this ITypeSymbol type)
    {
        return type.GetNullableUnderlyingType() ?? type;
    }

    /// <summary>
    /// A value that can carry a <c>null</c>: a reference type or a <see cref="System.Nullable{T}"/>.
    /// </summary>
    internal static bool CanBeNull(this ITypeSymbol type)
    {
        return type.IsValueType == false || type.GetNullableUnderlyingType() != null;
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
    /// A type a generated file can write down: the generated converter lives in the same assembly
    /// but in its own file and its own namespace, so every type it names has to be at least
    /// internal and has to mean the same thing there as it does where it was found. A type
    /// parameter does not - it only means something inside the declaration it belongs to.
    /// </summary>
    internal static bool IsVisibleToGeneratedCode(this ITypeSymbol type)
    {
        return type.IsAsAccessibleAs(Accessibility.Internal);
    }

    /// <summary>
    /// The same question asked of a public declaration. A public class can not take an internal
    /// type as a parameter or return one, so a converter for such a pair has to be internal
    /// itself - the compiler refuses the members otherwise.
    /// </summary>
    internal static bool IsPublicToGeneratedCode(this ITypeSymbol type)
    {
        return type.IsAsAccessibleAs(Accessibility.Public);
    }

    /// <summary>
    /// The type, everything it is built out of and everything it is nested in, all of them declared
    /// at least as accessible as <see cref="required"/>.
    /// </summary>
    private static bool IsAsAccessibleAs(this ITypeSymbol type, Accessibility required)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType.IsAsAccessibleAs(required);
        }

        if (type is not INamedTypeSymbol named)
        {
            // A type parameter, a pointer, dynamic: not a name a standalone file can carry.
            return false;
        }

        for (var current = named; current != null; current = current.ContainingType)
        {
            if (current.DeclaredAccessibility.IsAtLeast(required) == false)
            {
                return false;
            }
        }

        return named.TypeArguments.All(a => a.IsAsAccessibleAs(required));
    }

    private static bool IsAtLeast(this Accessibility declared, Accessibility required)
    {
        return declared == Accessibility.Public ||
               (declared == Accessibility.Internal && required == Accessibility.Internal);
    }

    internal static string ToFullyQualifiedName(this ITypeSymbol type)
    {
        return type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
    }

    /// <summary>
    /// The fully qualified name the way a person reads it, without the <c>global::</c> alias that
    /// only means something while the name is code.
    /// </summary>
    internal static string ToReadableName(this ITypeSymbol type)
    {
        return type.ToDisplayString(
            SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(
                SymbolDisplayGlobalNamespaceStyle.Omitted));
    }
}
