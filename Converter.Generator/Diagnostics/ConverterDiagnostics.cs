using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Diagnostics;

internal static class ConverterDiagnostics
{
    private const string Category = "Majipro.Converter";

    internal static readonly DiagnosticDescriptor UnhandledError = new DiagnosticDescriptor(
        "MC0001",
        "Unhandled error occured during converter generation",
        "Unhandled '{0}' occured during converter generation: {1}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// Two enums whose members do not line up. Anything the generator would write for the members
    /// that have no counterpart would be a guess, so it writes nothing and says so.
    /// </summary>
    internal static readonly DiagnosticDescriptor EnumMemberWithoutCounterpart = new DiagnosticDescriptor(
        "MC0002",
        "Enum converter can not be generated",
        "Can not convert enum '{0}' to '{1}', there is no counterpart in '{1}' for: {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// A target property nothing can fill: either the source has no property of that name, or it
    /// has one no value rule knows what to do with. Leaving it at its default would be a converter
    /// that silently loses a part of the target, so the generator writes nothing and says so.
    /// </summary>
    internal static readonly DiagnosticDescriptor PropertyWithoutCounterpart = new DiagnosticDescriptor(
        "MC0003",
        "Converter can not be generated",
        "Can not convert '{0}' to '{1}', there is no value in '{0}' for these properties of '{1}': {2}",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// A pair nobody answers: no rule knows how to write it, the library has no built-in converter
    /// for it, and nobody wrote one either. The call site would compile and then throw at runtime
    /// about a conversion that does not exist, so it fails the build instead, while the pair can
    /// still be dealt with.
    /// </summary>
    internal static readonly DiagnosticDescriptor ConversionWithoutConverter = new DiagnosticDescriptor(
        "MC0004",
        "Converter can not be generated",
        "Can not convert '{0}' to '{1}', this conversion can not be generated and no converter for it is implemented",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);

    /// <summary>
    /// A call site that names a type parameter, and no call closing it was found in the compilation:
    /// the method is reached some other way than by its name (a delegate, a base or an interface
    /// member, another assembly), or not at all. Which pair it asks for is decided only at runtime, so
    /// nothing could be generated for it and nothing can say whether a converter exists. A warning
    /// rather than an error, since a converter written by hand may well answer every pair it is ever
    /// called with - raise it with <c>dotnet_diagnostic.MC0005.severity = error</c> where that is
    /// not good enough.
    /// </summary>
    internal static readonly DiagnosticDescriptor UnresolvedGenericConversion = new DiagnosticDescriptor(
        "MC0005",
        "Generic conversion is not resolved at compile time",
        "Conversion '{0}' to '{1}' names a type parameter and no call of '{2}' closing it was found, so no converter is generated for it and the conversion may fail at runtime",
        Category,
        DiagnosticSeverity.Warning,
        isEnabledByDefault: true);

    /// <summary>
    /// A <c>RequiresConverterAttribute</c> naming something the method it is put on has no type
    /// parameter of. It is written as a string, and <c>nameof</c> only guards it where somebody used
    /// it, so a typo is possible - and ignoring it would leave the call to compile and then throw at
    /// runtime about a conversion nobody generated, which is what the attribute exists to prevent.
    /// </summary>
    internal static readonly DiagnosticDescriptor RequiresConverterWithoutTypeParameter = new DiagnosticDescriptor(
        "MC0006",
        "Converter requirement names no type parameter",
        "'{0}' requires a converter from '{1}' to '{2}', and '{3}' is not a type parameter of it",
        Category,
        DiagnosticSeverity.Error,
        isEnabledByDefault: true);
}
