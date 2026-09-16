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
}
