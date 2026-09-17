using System;
using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Generating;
using Majipro.Converter.Generator.Receivers;
using Majipro.Converter.Generator.Rules;
using Majipro.Converter.Generator.Sources;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator;

/// <summary>
/// Generates converter implementations for every
/// <c>IConvertingService.Convert&lt;TFrom, TTo&gt;(...)</c> call site found in the compilation.
/// Generated converters are plain public classes, so <c>DiCompositor.AddConverting</c> picks them up
/// by its regular assembly scan - no generated wiring is needed. A converter that turned out to be
/// able to fill a target somebody already holds is written as an
/// <see cref="IReferenceConverter{TFrom,TTo}"/>, which is both conversions in one class.
/// </summary>
/// <remarks>
/// There is no analysis pass and no model of a converter: the pairs are travelled one by one and
/// each one is written out on the spot, which is also when the pairs that converter needs are
/// requested. The queue is enumerated lazily, so those arrive in the same pass.
/// </remarks>
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
        catch (ConversionException e)
        {
            // A conversion that can not be written deterministically. The generation is over, and
            // the error stops the build - which is the point, the pair has to be dealt with by hand.
            context.ReportDiagnostic(Diagnostic.Create(
                e.Descriptor,
                Location.None,
                e.MessageArguments));
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
        var semantics = new ConversionSemantics(context.Compilation);

        if (semantics.IsAvailable == false)
        {
            // Majipro.Converter.Abstrations is not referenced, there is nothing to generate.
            return;
        }

        var queue = new ConversionQueue();
        var emitter = new ConverterEmitter(semantics, queue, GetBodyRules());

        foreach (var implemented in new ImplementedConverters(semantics).Pairs())
        {
            queue.Suppress(implemented);
        }

        foreach (var requested in new ConvertCallSites(semantics, receiver).Pairs())
        {
            queue.Request(requested);
        }

        foreach (var pair in queue.Travel())
        {
            foreach (var source in emitter.Emit(pair))
            {
                context.AddSource(source.FileName, source.Text);
            }
        }
    }

    /// <summary>
    /// The composition root. Order is meaning: the first rule that knows how to write the
    /// conversion, or the value of one property, is the one that writes it.
    /// </summary>
    private static IReadOnlyList<IConverterBodyRule> GetBodyRules()
    {
        var valueRules = new IPropertyValueRule[]
        {
            new DirectValueRule(),
            new ConvertedValueRule(),
            new CollectionValueRule()
        };

        return new IConverterBodyRule[]
        {
            new ToStringBodyRule(),
            new EnumBodyRule(),
            new ObjectInitializerBodyRule(valueRules)
        };
    }
}
