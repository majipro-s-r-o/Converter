using System.Collections.Generic;
using System.Linq;
using System.Text;
using Majipro.Converter.Generator.Conversions;
using Majipro.Converter.Generator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Majipro.Converter.Generator.Generating;

/// <summary>
/// One converter while it is being written. Everything a rule is allowed to know about the
/// conversion, and the two things it can do that reach outside of its own syntax node.
/// </summary>
/// <remarks>
/// <see cref="Convert"/> is the point of the whole design: writing the call to the converting
/// service is what requests the converter that call needs and what makes the field and the
/// constructor appear. There is no second place where the same fact is written down, so there is
/// nothing that can drift out of sync with what was actually generated.
/// </remarks>
internal sealed class ConverterContext
{
    private readonly ConversionQueue _queue;

    private Dictionary<string, IPropertySymbol>? _sourceProperties;
    private bool _usesConvertingService;

    public ConversionSemantics Semantics { get; }

    public ConversionPair Pair { get; }

    /// <summary>Type the properties are read from, so a <c>Nullable&lt;T&gt;</c> is unwrapped.</summary>
    public ITypeSymbol Source { get; }

    /// <summary>Type the object initializer creates, so never a <c>Nullable&lt;T&gt;</c>.</summary>
    public ITypeSymbol Target { get; }

    /// <summary><c>from</c>, or <c>from.Value</c> when the converter takes a nullable structure.</summary>
    public ExpressionSyntax SourceAccess { get; }

    /// <summary>True once something asked for a converted value, see the remarks on the class.</summary>
    public bool RequiresConvertingService => _usesConvertingService;

    public string ClassName { get; }

    public string FileName => ClassName + ".g.cs";

    public ConverterContext(ConversionSemantics semantics, ConversionQueue queue, ConversionPair pair)
    {
        Semantics = semantics;
        Pair = pair;

        _queue = queue;

        Source = pair.From.GetUnderlyingType();
        Target = pair.To.GetUnderlyingType();

        SourceAccess = pair.From.GetNullableUnderlyingType() == null
            ? ConverterSyntax.From()
            : ConverterSyntax.Member(ConverterSyntax.From(), nameof(System.Nullable<int>.Value));

        ClassName = GetClassName(pair);
    }

    /// <summary>
    /// The readable source property of that name, or nothing - which is a rule's way of saying the
    /// target property has no counterpart to map.
    /// </summary>
    public IEnumerable<IPropertySymbol> SourceProperty(string name)
    {
        _sourceProperties ??= Source
            .GetInstanceProperties()
            .Where(p => p.IsReadable())
            .ToDictionary(p => p.Name);

        if (_sourceProperties.TryGetValue(name, out var property))
        {
            yield return property;
        }
    }

    /// <summary><c>from.Property</c>, or <c>from.Value.Property</c>.</summary>
    public ExpressionSyntax Read(IPropertySymbol property)
    {
        return ConverterSyntax.Member(SourceAccess, property.Name);
    }

    /// <summary>
    /// Hands the value to the converting service, which both requests the converter that call needs
    /// and makes this converter take an <c>IConvertingService</c> of its own.
    /// </summary>
    public ExpressionSyntax Convert(ITypeSymbol from, ITypeSymbol to, ExpressionSyntax value)
    {
        _queue.Request(from, to);
        _usesConvertingService = true;

        return ConverterSyntax.ConvertCall(from, to, value);
    }

    private static string GetClassName(ConversionPair pair)
    {
        return Flatten(pair.From) + "To" + Flatten(pair.To) + "Converter";
    }

    /// <summary>
    /// Turns a fully qualified name into an identifier. <c>?</c> becomes <c>Nullable</c> so that
    /// <c>Guid</c> and <c>Guid?</c> do not end up claiming the same class name.
    /// </summary>
    private static string Flatten(ITypeSymbol type)
    {
        var fullName = type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
        var result = new StringBuilder();
        var capitalize = true;

        foreach (var character in fullName.Replace("global::", string.Empty))
        {
            if (character == '?')
            {
                result.Append("Nullable");
            }
            else if (char.IsLetterOrDigit(character) || character == '_')
            {
                // Keywords like 'int' and 'string' are written in lower case, which would leave
                // the generated class name starting with one.
                result.Append(capitalize ? char.ToUpperInvariant(character) : character);
                capitalize = false;

                continue;
            }

            capitalize = true;
        }

        return result.ToString();
    }
}
