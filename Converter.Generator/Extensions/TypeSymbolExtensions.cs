using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Extensions;

internal static class TypeSymbolExtensions
{
    /// <summary>
    /// How deeply the type arguments of a name may nest before the generator stops treating it as a
    /// type it maps. A termination bound rather than a judgement about the type, see the remarks on
    /// <see cref="IsMappableSource"/>: eight is several times deeper than anything worth converting,
    /// and shallow enough that a type growing out of its own definition ends the pass instead of
    /// travelling forever.
    /// </summary>
    private const int MaxTypeArgumentDepth = 8;

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
    /// Types the generator is able to read from: a class or structure that carries what it means in
    /// its properties. A generic type is one of them as long as it is closed, which is
    /// <see cref="IsVisibleToGeneratedCode"/>'s answer rather than a question of its own - a type
    /// argument that is a type parameter is not a name a generated file can carry, while
    /// <c>Wrapper&lt;Guid, Person&gt;</c> is a name like any other.
    /// </summary>
    /// <remarks>
    /// Two shapes are refused although they are classes with properties.
    ///
    /// A collection means the items it holds rather than the properties it declares, and those
    /// properties are no substitute for them: mapping a <c>List&lt;T&gt;</c> as an object would copy
    /// its capacity and quietly return an empty list. Items are
    /// <see cref="Rules.CollectionValueRule"/>'s business, and asking this here is what keeps one
    /// type from meaning two different things.
    ///
    /// A name nested deeper than <see cref="MaxTypeArgumentDepth"/> is refused because closed
    /// generics are the point at which the pairs stop being a finite set. A <c>Node&lt;T&gt;</c>
    /// with a property of type <c>Node&lt;Node&lt;T&gt;&gt;</c> asks for a converter whose own
    /// property asks for the next one, forever, and every one of those is a pair
    /// <see cref="Conversions.ConversionQueue"/> has not handled yet - progress, as far as it can
    /// tell. A compilation declares only so many generic definitions, so bounding how deeply their
    /// arguments may nest is what makes the names finite again and the travelling end. What falls
    /// outside the bound fails the build the way any other pair no rule claims does, which is the
    /// point: a generation that never comes back is the one outcome nothing can be done about.
    /// </remarks>
    internal static bool IsMappableSource(this ITypeSymbol type)
    {
        return type is INamedTypeSymbol named &&
               named.SpecialType == SpecialType.None &&
               (named.TypeKind == TypeKind.Class || named.TypeKind == TypeKind.Struct) &&
               named.IsCollection() == false &&
               named.GetTypeArgumentDepth() <= MaxTypeArgumentDepth &&
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
    /// A type that means the items it holds rather than the properties it declares. Asked of the non
    /// generic <see cref="System.Collections.IEnumerable"/>, which is every collection there is, and
    /// which takes nothing but the type itself to answer.
    /// </summary>
    private static bool IsCollection(this INamedTypeSymbol type)
    {
        return type.AllInterfaces.Any(i => i.SpecialType == SpecialType.System_Collections_IEnumerable);
    }

    /// <summary>
    /// How deeply the type arguments of a name are nested: <c>Person</c> is none,
    /// <c>Wrapper&lt;Person&gt;</c> is one, <c>Wrapper&lt;Wrapper&lt;Person&gt;&gt;</c> is two. Every
    /// part of the name counts, the arguments of what it is nested in included.
    /// </summary>
    private static int GetTypeArgumentDepth(this ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType.GetTypeArgumentDepth();
        }

        if (type is not INamedTypeSymbol named)
        {
            return 0;
        }

        var depth = 0;

        for (var current = named; current != null; current = current.ContainingType)
        {
            foreach (var argument in current.TypeArguments)
            {
                depth = Math.Max(depth, argument.GetTypeArgumentDepth() + 1);
            }
        }

        return depth;
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

            // The arguments of every level, not only of the outermost one: naming
            // Outer<Secret>.Inner means writing Secret down as well. A type parameter is refused
            // here, which is what makes a closed generic type the only generic one that is mappable.
            if (current.TypeArguments.All(a => a.IsAsAccessibleAs(required)) == false)
            {
                return false;
            }
        }

        return true;
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
