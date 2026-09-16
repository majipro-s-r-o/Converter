using System;
using Majipro.Converter.Generator.Analysis;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Generating;
using Majipro.Converter.Generator.Receivers;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator;

/// <summary>
/// Generates <see cref="IConverter{TFrom,TTo}"/> implementations for every
/// <c>IConvertingService.Convert&lt;TFrom, TTo&gt;(...)</c> call site found in the compilation.
/// Generated converters are plain public classes, so <c>DiCompositor.AddConverting</c> picks them up
/// by its regular assembly scan - no generated wiring is needed.
/// </summary>
[Generator]
internal class ConverterGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new ConvertCallsSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not ConvertCallsSyntaxReceiver receiver)
        {
            return;
        }

        try
        {
            ExecuteInternal(context, receiver);
        }
        catch (Exception e)
        {
            // A generator that throws kills the build with an unspecific error, report it instead.
            context.ReportDiagnostic(Diagnostic.Create(
                ConverterDiagnostics.UnhandledError,
                Location.None,
                e.GetType().Name,
                e.Message));
        }
    }

    private static void ExecuteInternal(GeneratorExecutionContext context, ConvertCallsSyntaxReceiver receiver)
    {
        var conversions = new ConversionAnalyzer(context.Compilation).Analyze(receiver.ConvertCalls);

        foreach (var conversion in conversions)
        {
            context.AddSource(conversion.FileName, ConverterSourceBuilder.Build(conversion));
        }
    }
}
