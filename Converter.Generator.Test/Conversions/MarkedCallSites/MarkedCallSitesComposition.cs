using System;
using System.Collections.Generic;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Extensions;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.MarkedCallSites;

/// <summary>
/// Calls of methods that are not on <c>IConvertingService</c> and require a converter all the same,
/// because they say so with <c>RequiresConverterAttribute</c>. The generator never sees the body of
/// any of them doing a conversion, so the attribute is the only thing that can make the pair known.
/// </summary>
/// <remarks>
/// The attributes of this file write the names as strings: <c>nameof</c> of a method's own type
/// parameter inside an attribute on it is C# 11, and the suite compiles this file with the oldest
/// Roslyn the generator supports, which does not have it - the same thing a consumer on an older
/// compiler has to write. The types this file declares besides the composition are <c>internal</c>,
/// because the file is compiled into the test assembly too, and a public copy of the extension class
/// would make its call ambiguous.
/// </remarks>
public class MarkedCallSitesComposition : TestCompositionBase
{
    /// <summary><c>ConvertExplicitly</c> lives in Majipro.Converter, the type arguments inferred.</summary>
    public List<MarkedCallSitesTestCase.ExplicitListTo> ConvertExplicitList(
        IConvertingService convertingService,
        List<MarkedCallSitesTestCase.ExplicitListFrom> from)
    {
        return convertingService.ConvertExplicitly(from, () => new List<MarkedCallSitesTestCase.ExplicitListTo>());
    }

    /// <summary><c>ConvertExplicitly</c> again, the type arguments written out.</summary>
    public MarkedCallSitesTestCase.ExplicitArrayTo[] ConvertExplicitArray(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.ExplicitArrayFrom[] from)
    {
        return convertingService.ConvertExplicitly<MarkedCallSitesTestCase.ExplicitArrayFrom, MarkedCallSitesTestCase.ExplicitArrayTo>(from);
    }

    /// <summary>A marked method of a referenced assembly.</summary>
    public MarkedCallSitesTestCase.ReferencedTo ConvertReferenced(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.ReferencedFrom from)
    {
        return MarkedCallSitesTestCase.Relay<MarkedCallSitesTestCase.ReferencedTo, MarkedCallSitesTestCase.ReferencedFrom>(
            convertingService,
            from);
    }

    /// <summary>A marked method of a referenced assembly, declared in a nested type.</summary>
    public MarkedCallSitesTestCase.NestedTo ConvertNested(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.NestedFrom from)
    {
        return MarkedCallSitesTestCase.Helpers.NestedRelay<MarkedCallSitesTestCase.NestedFrom, MarkedCallSitesTestCase.NestedTo>(
            convertingService,
            from);
    }

    /// <summary>A marked method of this assembly, called with the type arguments inferred.</summary>
    public MarkedCallSitesTestCase.LocalTo ConvertLocal(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.LocalFrom from)
    {
        return Map(convertingService, from, default(MarkedCallSitesTestCase.LocalTo));
    }

    /// <summary>
    /// A marked method called with a type parameter of the method it is called from, which leaves the
    /// pair open until <c>GenericCallSites</c> closes it through the call of <see cref="Wrap{T}"/>.
    /// </summary>
    public MarkedCallSitesTestCase.GenericTo ConvertGeneric(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.GenericFrom from)
    {
        return Wrap(convertingService, from);
    }

    /// <summary>A marked extension method of this assembly, bound in its reduced form.</summary>
    public MarkedCallSitesTestCase.ExtensionTo ConvertExtension(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.ExtensionFrom from)
    {
        return convertingService.MapWith<MarkedCallSitesTestCase.ExtensionFrom, MarkedCallSitesTestCase.ExtensionTo>(from);
    }

    /// <summary>
    /// The interface member is marked, the call is written against the class implementing it and binds
    /// to the implementation, which is not.
    /// </summary>
    public MarkedCallSitesTestCase.ContractTo ConvertThroughImplementation(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.ContractFrom from)
    {
        return new MarkedImplementation().Map<MarkedCallSitesTestCase.ContractFrom, MarkedCallSitesTestCase.ContractTo>(
            convertingService,
            from);
    }

    /// <summary>The abstract member is marked, the call binds to the override, which is not.</summary>
    public MarkedCallSitesTestCase.OverrideTo ConvertThroughOverride(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.OverrideFrom from)
    {
        return new MarkedOverride().Map<MarkedCallSitesTestCase.OverrideFrom, MarkedCallSitesTestCase.OverrideTo>(
            convertingService,
            from);
    }

    /// <summary>The attribute names a type parameter of the class the marked method is a member of.</summary>
    public MarkedCallSitesTestCase.ClassLevelTo ConvertClassLevel(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.ClassLevelFrom from)
    {
        return new MarkedBox<MarkedCallSitesTestCase.ClassLevelFrom>().To<MarkedCallSitesTestCase.ClassLevelTo>(
            convertingService,
            from);
    }

    /// <summary>A marked local function, which is no member of any type.</summary>
    public MarkedCallSitesTestCase.LocalFunctionTo ConvertLocalFunction(
        IConvertingService convertingService,
        MarkedCallSitesTestCase.LocalFunctionFrom from)
    {
        return Local<MarkedCallSitesTestCase.LocalFunctionFrom, MarkedCallSitesTestCase.LocalFunctionTo>(convertingService, from);

        [RequiresConverter("TIn", "TOut")]
        static TOut Local<TIn, TOut>(IConvertingService convertingService, TIn from)
        {
            return Unseen<TIn, TOut>(convertingService, from);
        }
    }

    [RequiresConverter("TIn", "TOut")]
    public static TOut Map<TIn, TOut>(IConvertingService convertingService, TIn from, TOut _)
    {
        return Unseen<TIn, TOut>(convertingService, from);
    }

    private static MarkedCallSitesTestCase.GenericTo Wrap<T>(IConvertingService convertingService, T value)
    {
        return MarkedCallSitesTestCase.Relay<MarkedCallSitesTestCase.GenericTo, T>(convertingService, value);
    }

    /// <summary>
    /// The conversion handed over as a method group rather than called, which is a shape
    /// <c>GenericCallSites</c> does not follow - so whatever the marked methods above get generated,
    /// it is their attribute that asked for it, never their body.
    /// </summary>
    internal static TOut Unseen<TIn, TOut>(IConvertingService convertingService, TIn from)
    {
        Func<TIn, TOut> convert = convertingService.Convert<TIn, TOut>;

        return convert(from);
    }
}

internal static class MarkedExtensions
{
    [RequiresConverter("TFrom", "TTo")]
    public static TTo MapWith<TFrom, TTo>(this IConvertingService convertingService, TFrom from)
    {
        return MarkedCallSitesComposition.Unseen<TFrom, TTo>(convertingService, from);
    }
}

internal interface IMarkedContract
{
    [RequiresConverter("TIn", "TOut")]
    TOut Map<TIn, TOut>(IConvertingService convertingService, TIn from);
}

/// <summary>Names its type parameters differently from the contract, they are the same ones by position.</summary>
internal sealed class MarkedImplementation : IMarkedContract
{
    public TB Map<TA, TB>(IConvertingService convertingService, TA from)
    {
        return MarkedCallSitesComposition.Unseen<TA, TB>(convertingService, from);
    }
}

internal abstract class MarkedBase
{
    [RequiresConverter("TIn", "TOut")]
    public abstract TOut Map<TIn, TOut>(IConvertingService convertingService, TIn from);
}

/// <summary>Names its type parameters differently from the member it overrides.</summary>
internal sealed class MarkedOverride : MarkedBase
{
    public override TY Map<TX, TY>(IConvertingService convertingService, TX from)
    {
        return MarkedCallSitesComposition.Unseen<TX, TY>(convertingService, from);
    }
}

internal sealed class MarkedBox<TIn>
{
    [RequiresConverter("TIn", "TOut")]
    public TOut To<TOut>(IConvertingService convertingService, TIn from)
    {
        return MarkedCallSitesComposition.Unseen<TIn, TOut>(convertingService, from);
    }
}
