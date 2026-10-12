using System;
using System.Collections.Generic;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.MarkedGenericCallSite;

/// <summary>
/// Generic call sites nothing in this compilation calls, written inside methods that say with
/// <c>RequiresConverterAttribute</c> which pair they ask for. That is the shape of a generic helper in a
/// class library: its callers are in other assemblies, and the attribute is what hands the pair to
/// them - so the call site is not unresolved and there is nothing to warn about. One method asks for a
/// pair it does not declare, which keeps its warning.
/// </summary>
/// <remarks>
/// The names are strings because this file compiles on Roslyn 4.3.1, where <c>nameof</c> of a method's
/// own type parameter in an attribute on it does not exist yet. The other types are <c>internal</c>
/// because the file is compiled into the test assembly as well.
/// </remarks>
public class MarkedGenericCallSiteComposition : TestCompositionBase
{
    /// <summary>The ordinary pair, still generated next to all of it.</summary>
    public MarkedGenericCallSiteTestCase.To Convert(
        IConvertingService convertingService,
        MarkedGenericCallSiteTestCase.From from)
    {
        return convertingService.Convert<MarkedGenericCallSiteTestCase.From, MarkedGenericCallSiteTestCase.To>(from);
    }

    [RequiresConverter("TFrom", "TTo")]
    public static TTo Declared<TFrom, TTo>(IConvertingService convertingService, TFrom from)
    {
        return convertingService.Convert<TFrom, TTo>(from);
    }

    /// <summary>The call site is in a lambda, whose owner is the method around it.</summary>
    [RequiresConverter("TFrom", "TTo")]
    public static Func<TFrom, TTo> DeclaredInLambda<TFrom, TTo>(IConvertingService convertingService)
    {
        return from => convertingService.Convert<TFrom, TTo>(from);
    }

    /// <summary>The call site is in a local function, using the type parameters of the method around it.</summary>
    [RequiresConverter("TFrom", "TTo")]
    public static TTo DeclaredInLocalFunction<TFrom, TTo>(IConvertingService convertingService, TFrom from)
    {
        return Local();

        TTo Local()
        {
            return convertingService.Convert<TFrom, TTo>(from);
        }
    }

    /// <summary>
    /// Says it requires <c>TFrom -&gt; TTo</c> and asks for <c>TFrom -&gt; List&lt;TTo&gt;</c>. The
    /// callers request the pair the attribute names, not this one, so this one is still unresolved.
    /// </summary>
    [RequiresConverter("TFrom", "TTo")]
    public static List<TTo> NotDeclared<TFrom, TTo>(IConvertingService convertingService, TFrom from)
    {
        return convertingService.Convert<TFrom, List<TTo>>(from);
    }
}

internal interface IMarkedProvider
{
    [RequiresConverter("TIn", "TOut")]
    TOut Find<TIn, TOut>(IConvertingService convertingService, TIn from);
}

/// <summary>
/// Not marked itself and naming its type parameters differently: the contract says what it requires.
/// Every call goes through the interface, so nothing calls this method by its name - the shape of a
/// provider registered in a container.
/// </summary>
internal sealed class MarkedProvider : IMarkedProvider
{
    public TB Find<TA, TB>(IConvertingService convertingService, TA from)
    {
        return convertingService.Convert<TA, TB>(from);
    }
}

/// <summary>The attribute names a type parameter of the class.</summary>
internal sealed class MarkedBox<TIn>
{
    [RequiresConverter("TIn", "TOut")]
    public TOut To<TOut>(IConvertingService convertingService, TIn from)
    {
        return convertingService.Convert<TIn, TOut>(from);
    }
}
