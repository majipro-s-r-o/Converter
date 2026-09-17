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
}
