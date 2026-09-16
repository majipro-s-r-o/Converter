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
}
