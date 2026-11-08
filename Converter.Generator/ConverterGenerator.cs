using System;
using System.Collections.Generic;
using Majipro.Converter.Generator.Conversions;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Generating;
using Majipro.Converter.Generator.Razor;
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
        context.RegisterForSyntaxNotifications(() => new InvocationsSyntaxReceiver());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not InvocationsSyntaxReceiver receiver)
        {
            return;
        }

        try
        {
            ExecuteInternal(context, receiver);
        }
        catch (ConversionException e)
        {
            // What is left to reach here is a call site that names no conversion at all (MC0006) -
            // a refused pair is reported where it is travelled. The generation is over, and the
            // error stops the build.
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

    private static void ExecuteInternal(GeneratorExecutionContext context, InvocationsSyntaxReceiver receiver)
    {
        // The call sites written in .razor and .cshtml files are in no tree of the compilation - the
        // Razor generator's output is invisible to every other generator - so Razor's generator is
        // run once more, and its C# is bound against a compilation of its own.
        var razor = RazorSources.Create(context, receiver);

        if (razor.Problem != null)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ConverterDiagnostics.RazorNotCompiled,
                Location.None,
                razor.Problem));
        }

        foreach (var tree in razor.SyntaxTrees)
        {
            foreach (var node in tree.GetRoot(context.CancellationToken).DescendantNodesAndSelf())
            {
                receiver.OnVisitSyntaxNode(node);
            }
        }

        var semantics = new ConversionSemantics(razor.AddTo(context.Compilation));

        if (semantics.IsAvailable == false)
        {
            // Majipro.Converter.Abstractions is not referenced, there is nothing to generate.
            return;
        }

        var queue = new ConversionQueue();
        var emitter = new ConverterEmitter(semantics, queue, GetBodyRules());

        foreach (var implemented in new ImplementedConverters(semantics).Pairs())
        {
            queue.Suppress(implemented);
        }

        var callSites = new ConvertCallSites(semantics, receiver);
        var genericCallSites = new GenericCallSites(semantics, callSites, receiver.Invocations);

        foreach (var requested in callSites.Pairs())
        {
            queue.Request(requested);
        }

        foreach (var requested in genericCallSites.Pairs())
        {
            queue.Request(requested);
        }

        foreach (var unresolved in genericCallSites.Unresolved)
        {
            context.ReportDiagnostic(Diagnostic.Create(
                ConverterDiagnostics.UnresolvedGenericConversion,
                razor.Map(unresolved.CallSite.Node.GetLocation()),
                unresolved.CallSite.Pair.From.ToReadableName(),
                unresolved.CallSite.Pair.To.ToReadableName(),
                unresolved.Method?.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat) ??
                unresolved.CallSite.Node.ToString()));
        }

        foreach (var pair in queue.Travel())
        {
            try
            {
                foreach (var source in emitter.Emit(pair))
                {
                    context.AddSource(source.FileName, source.Text);
                }
            }
            catch (ConversionException e)
            {
                // A pair that can not be written deterministically fails the build, but only that
                // pair: the rest are still travelled, so every pair that has to be dealt with by hand
                // is reported in one build instead of one per build.
                context.ReportDiagnostic(Diagnostic.Create(
                    e.Descriptor,
                    Location.None,
                    e.MessageArguments));
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
            new EnumValueRule(),
            new CollectionValueRule(),
            new StringValueRule()
        };

        return new IConverterBodyRule[]
        {
            new ToStringBodyRule(),
            new EnumBodyRule(),
            new ObjectInitializerBodyRule(valueRules),
            new DirectBodyRule()
        };
    }
}
