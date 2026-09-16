using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Majipro.Converter.Generator.Conversions;

/// <summary>
/// What the compilation knows: the converter interfaces the generator looks for, and the questions
/// about types that only the compiler can answer. Every symbol is resolved by the metadata name of
/// the type the generator itself was compiled against, so there is no name kept in sync by hand.
/// </summary>
internal sealed class ConversionSemantics
{
    private readonly Compilation _compilation;

    /// <summary>The compilation again, typed, because only the C# one classifies conversions.</summary>
    private readonly CSharpCompilation? _cSharpCompilation;

    private readonly Dictionary<SyntaxTree, SemanticModel> _semanticModels =
        new Dictionary<SyntaxTree, SemanticModel>();

    private readonly INamedTypeSymbol? _enumerable;
    private readonly IReadOnlyList<INamedTypeSymbol> _listLikeTargets;
    private readonly IReadOnlyList<INamedTypeSymbol> _builtInStringTargets;
    private readonly IReadOnlyList<INamedTypeSymbol> _scalarStructures;

    /// <summary>
    /// Values the compiler knows by a special type of their own, all of them written and read as a
    /// single value. <see cref="System.Guid"/> and friends have no special type and are resolved as
    /// symbols instead, see <see cref="_scalarStructures"/>.
    /// </summary>
    private static readonly HashSet<SpecialType> ScalarSpecialTypes = new HashSet<SpecialType>
    {
        SpecialType.System_Boolean,
        SpecialType.System_Char,
        SpecialType.System_SByte,
        SpecialType.System_Byte,
        SpecialType.System_Int16,
        SpecialType.System_UInt16,
        SpecialType.System_Int32,
        SpecialType.System_UInt32,
        SpecialType.System_Int64,
        SpecialType.System_UInt64,
        SpecialType.System_Single,
        SpecialType.System_Double,
        SpecialType.System_Decimal,
        SpecialType.System_DateTime
    };

    public INamedTypeSymbol? ConvertingService { get; }

    public INamedTypeSymbol? ConverterInterface { get; }

    public INamedTypeSymbol? AsyncConverterInterface { get; }

    public INamedTypeSymbol? ReferenceConverterInterface { get; }

    /// <summary>
    /// The assemblies a converter written by hand can live in: the one being compiled and every
    /// referenced assembly that references the abstractions. <c>DiCompositor.AddConverting</c>
    /// finds converters by reflecting over the assemblies it is handed, so a converter in a class
    /// library is as real as one written next to the call site - the generator has to ask the same
    /// question. An assembly that does not reference <c>Majipro.Converter.Abstrations</c> can not
    /// implement a converter interface, which keeps this to the one or two assemblies that do
    /// instead of the whole framework.
    /// </summary>
    public IReadOnlyList<IAssemblySymbol> ConverterAssemblies { get; }

    /// <summary>
    /// Majipro.Converter.Abstrations is referenced, so there is something to generate at all.
    /// </summary>
    public bool IsAvailable => ConvertingService != null && ConverterInterface != null;

    public ConversionSemantics(Compilation compilation)
    {
        _compilation = compilation;
        _cSharpCompilation = compilation as CSharpCompilation;

        ConvertingService = GetTypeSymbol(compilation, typeof(IConvertingService));
        ConverterInterface = GetTypeSymbol(compilation, typeof(IConverter<,>));
        AsyncConverterInterface = GetTypeSymbol(compilation, typeof(IAsyncConverter<,>));
        ReferenceConverterInterface = GetTypeSymbol(compilation, typeof(IReferenceConverter<,>));

        _enumerable = GetTypeSymbol(compilation, typeof(IEnumerable<>));

        // Collection properties are materialized as a List<T>, so every target type a List<T> fits into.
        _listLikeTargets = GetTypeSymbols(
            compilation,
            typeof(IEnumerable<>),
            typeof(ICollection<>),
            typeof(IList<>),
            typeof(IReadOnlyCollection<>),
            typeof(IReadOnlyList<>),
            typeof(List<>));

        // The types Converter/Converters has a built-in IConverter<string, T> for, see
        // DiCompositorConverters.AddBuildInConverters. This is that list a second time on purpose:
        // an analyzer can not reference the library that registers them, so adding a built-in
        // converter means adding its target here as well or a property never reaches it.
        _builtInStringTargets = GetTypeSymbols(
            compilation,
            typeof(bool),
            typeof(byte),
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(long));

        // Structures that are a single value, the way a primitive is. They carry no special type,
        // so they are the ones that have to be resolved as symbols.
        _scalarStructures = GetTypeSymbols(
            compilation,
            typeof(Guid),
            typeof(DateTimeOffset),
            typeof(TimeSpan));

        ConverterAssemblies = GetConverterAssemblies(compilation, ConverterInterface?.ContainingAssembly);
    }

    /// <summary>
    /// <see cref="ConverterAssemblies"/>: the compiled assembly first, then the referenced ones
    /// that reference the abstractions. The abstractions assembly itself is skipped, it declares the
    /// interfaces and implements none of them.
    /// </summary>
    private static IReadOnlyList<IAssemblySymbol> GetConverterAssemblies(
        Compilation compilation,
        IAssemblySymbol? abstractions)
    {
        var assemblies = new List<IAssemblySymbol>
        {
            compilation.Assembly
        };

        if (abstractions == null)
        {
            return assemblies;
        }

        foreach (var referenced in compilation.SourceModule.ReferencedAssemblySymbols)
        {
            if (SymbolEqualityComparer.Default.Equals(referenced, abstractions))
            {
                continue;
            }

            var referencesAbstrations = referenced.Modules.Any(m => m.ReferencedAssemblySymbols.Any(
                r => SymbolEqualityComparer.Default.Equals(r, abstractions)));

            if (referencesAbstrations)
            {
                assemblies.Add(referenced);
            }
        }

        return assemblies;
    }

    /// <summary>
    /// The interface the generated class implements, closed over the pair:
    /// <c>IReferenceConverter&lt;TFrom, TTo&gt;</c> when the converter also fills a target the
    /// caller already holds, <c>IConverter&lt;TFrom, TTo&gt;</c> when it only creates one. The
    /// first one extends the second, so there is never a class implementing both by name - which is
    /// also what keeps DiCompositionValidator from seeing two registrations for one pair.
    /// </summary>
    public INamedTypeSymbol Converter(ConversionPair pair, bool reference)
    {
        var definition = reference && ReferenceConverterInterface != null
            ? ReferenceConverterInterface
            : ConverterInterface!;

        return definition.Construct(pair.From, pair.To);
    }

    /// <summary>
    /// Whether the compiler would take the source value for the target property without a cast.
    /// </summary>
    public bool IsAssignable(ITypeSymbol source, ITypeSymbol target)
    {
        return _cSharpCompilation == null
            ? SymbolEqualityComparer.Default.Equals(source, target)
            : _cSharpCompilation.ClassifyConversion(source, target).IsImplicit;
    }

    /// <summary>
    /// <c>T</c> of the <see cref="IEnumerable{T}"/> the type is. A <c>string</c> is an
    /// <see cref="IEnumerable{T}"/> of characters and is deliberately not treated as a collection.
    /// </summary>
    public ITypeSymbol? GetEnumeratedType(ITypeSymbol type)
    {
        if (type is IArrayTypeSymbol array)
        {
            return array.ElementType;
        }

        if (_enumerable == null || type.SpecialType == SpecialType.System_String)
        {
            return null;
        }

        if (type is INamedTypeSymbol named &&
            SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, _enumerable))
        {
            return named.TypeArguments[0];
        }

        foreach (var iface in type.AllInterfaces)
        {
            if (SymbolEqualityComparer.Default.Equals(iface.OriginalDefinition, _enumerable))
            {
                return iface.TypeArguments[0];
            }
        }

        return null;
    }

    /// <summary>
    /// <c>T</c> of a property a freshly built <c>List&lt;T&gt;</c> can be assigned to.
    /// </summary>
    public ITypeSymbol? GetListItemType(ITypeSymbol type)
    {
        if (type is not INamedTypeSymbol named || named.TypeArguments.Length != 1)
        {
            return null;
        }

        return _listLikeTargets.Any(t => SymbolEqualityComparer.Default.Equals(named.OriginalDefinition, t))
            ? named.TypeArguments[0]
            : null;
    }

    public SemanticModel GetSemanticModel(SyntaxTree syntaxTree)
    {
        if (_semanticModels.TryGetValue(syntaxTree, out var semanticModel) == false)
        {
            semanticModel = _compilation.GetSemanticModel(syntaxTree);
            _semanticModels.Add(syntaxTree, semanticModel);
        }

        return semanticModel;
    }

    /// <summary>
    /// A scalar and its text form, the pair <see cref="Rules.StringValueRule"/> writes for a
    /// property and <see cref="Rules.CollectionValueRule"/> accepts for the items of a collection.
    /// Whoever asks this is about to hand the value to the converting service, so the question it
    /// really answers is whether a converter for the pair is going to be there.
    /// </summary>
    /// <remarks>
    /// The two directions are there for different reasons.
    ///
    /// Writing a value out is <see cref="Rules.ToStringBodyRule"/>: requesting the pair is all it
    /// takes, the converter is written in the same pass. Any type a generated file can name would
    /// produce one, and only a scalar is accepted anyway - <c>Convert.ToString</c> of a structure
    /// somebody declared is its type name, and a converter quietly returning that is worse than the
    /// build error the pair gets without this.
    ///
    /// Reading one back is the opposite: no body rule knows how to parse, so nothing is generated
    /// for <c>string -&gt; int</c> and the pair is answered by the built-in converter
    /// <c>DiCompositor.AddConverting</c> registers. So it is accepted only for the types the library
    /// really has one for, asked as they were declared rather than unwrapped: the registration is
    /// <c>IConverter&lt;string, int&gt;</c> and nothing answers <c>string -&gt; int?</c>. Whether an
    /// unparseable string should make such a target <c>0</c>, the way the built-in answers, or
    /// <c>null</c>, the way the target reads, is a decision nobody has made - until somebody makes
    /// it, that pair keeps failing the build instead of being given an answer picked here.
    /// </remarks>
    public bool IsTextConversion(ITypeSymbol from, ITypeSymbol to)
    {
        if (IsBuiltInStringConversion(from, to))
        {
            return true;
        }

        // The converter for this one is generated, so the source has to be a type the generated
        // file can name - the same thing IsMappableSource asks of an ordinary converted value.
        return to.SpecialType == SpecialType.System_String &&
               from.IsVisibleToGeneratedCode() &&
               IsScalar(from.GetUnderlyingType());
    }

    /// <summary>
    /// Whether the library has a built-in converter for this pair: a string and one of the types
    /// <c>Converter/Converters</c> reads it into.
    /// </summary>
    public bool IsBuiltInStringConversion(ITypeSymbol from, ITypeSymbol to)
    {
        return from.SpecialType == SpecialType.System_String &&
               _builtInStringTargets.Any(t => SymbolEqualityComparer.Default.Equals(to, t));
    }

    /// <summary>
    /// A value with a text form of its own: an enum, a primitive, or one of the framework
    /// structures written as a single value.
    /// </summary>
    private bool IsScalar(ITypeSymbol type)
    {
        return type.TypeKind == TypeKind.Enum ||
               ScalarSpecialTypes.Contains(type.SpecialType) ||
               _scalarStructures.Any(s => SymbolEqualityComparer.Default.Equals(type, s));
    }

    private static IReadOnlyList<INamedTypeSymbol> GetTypeSymbols(Compilation compilation, params Type[] types)
    {
        return types
            .Select(t => GetTypeSymbol(compilation, t))
            .Where(s => s != null)
            .Select(s => s!)
            .ToList();
    }

    private static INamedTypeSymbol? GetTypeSymbol(Compilation compilation, Type type)
    {
        return type.FullName == null
            ? null
            : compilation.GetTypeByMetadataName(type.FullName);
    }
}
