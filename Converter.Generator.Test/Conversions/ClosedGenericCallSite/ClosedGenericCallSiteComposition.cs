using System;
using System.Collections.Generic;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.ClosedGenericCallSite;

/// <summary>
/// Call sites that name a type parameter, each closed by whoever calls the method they are written
/// in. None of them writes a pair down, and every one of them asks for one at runtime.
/// </summary>
public class ClosedGenericCallSiteComposition : TestCompositionBase
{
    public Dictionary<string, ClosedGenericCallSiteTestCase.VariableKind> ConvertNotificationKeys(
        IConvertingService convertingService,
        IEnumerable<ClosedGenericCallSiteTestCase.NotificationVariable> variables)
    {
        var tree = new Dictionary<string, ClosedGenericCallSiteTestCase.VariableKind>();

        foreach (var variable in variables)
        {
            BuildKeyTree(convertingService, variable, v => v.Kind, variable.Key, tree);
        }

        return tree;
    }

    /// <summary>
    /// The same helper closed over a second enum, whose converter is written by hand below - so the
    /// pair it closes into is suppressed, as any other pair somebody wrote is.
    /// </summary>
    public Dictionary<string, ClosedGenericCallSiteTestCase.VariableKind> ConvertPrintKeys(
        IConvertingService convertingService,
        IEnumerable<ClosedGenericCallSiteTestCase.PrintVariable> variables)
    {
        var tree = new Dictionary<string, ClosedGenericCallSiteTestCase.VariableKind>();

        foreach (var variable in variables)
        {
            BuildKeyTree(convertingService, variable, v => v.Kind, variable.Key, tree);
        }

        return tree;
    }

    /// <summary>
    /// Type arguments inferred from a lambda, and a recursive call that hands them on unchanged.
    /// </summary>
    private void BuildKeyTree<TVariable, TKind>(
        IConvertingService convertingService,
        TVariable variable,
        Func<TVariable, TKind> kindSelector,
        string key,
        Dictionary<string, ClosedGenericCallSiteTestCase.VariableKind> tree)
    {
        var separatorIndex = key.IndexOf('.');

        if (separatorIndex > 0)
        {
            BuildKeyTree(convertingService, variable, kindSelector, key.Substring(separatorIndex + 1), tree);

            return;
        }

        tree.Add(key, convertingService.Convert<TKind, ClosedGenericCallSiteTestCase.VariableKind>(kindSelector(variable)));
    }

    /// <summary>A generic method handing its type parameter on to another one, renamed.</summary>
    public ClosedGenericCallSiteTestCase.VariableKind ConvertRenamed(
        IConvertingService convertingService,
        ClosedGenericCallSiteTestCase.RenamedKind kind)
    {
        return Outer(convertingService, kind);
    }

    private ClosedGenericCallSiteTestCase.VariableKind Outer<TOuter>(IConvertingService convertingService, TOuter kind)
    {
        return Inner(convertingService, kind);
    }

    private ClosedGenericCallSiteTestCase.VariableKind Inner<TInner>(IConvertingService convertingService, TInner kind)
    {
        return convertingService.Convert<TInner, ClosedGenericCallSiteTestCase.VariableKind>(kind);
    }

    public ClosedGenericCallSiteTestCase.VariableKind ConvertByExtension(
        IConvertingService convertingService,
        ClosedGenericCallSiteTestCase.ExtensionKind kind)
    {
        return kind.ToVariableKind(convertingService);
    }

    public ClosedGenericCallSiteTestCase.VariableKind ConvertByMapper(
        IConvertingService convertingService,
        ClosedGenericCallSiteTestCase.MapperKind kind)
    {
        return new KindMapper<ClosedGenericCallSiteTestCase.MapperKind>(convertingService).Map(kind);
    }

    public ClosedGenericCallSiteTestCase.VariableKind ConvertByLocalFunction(
        IConvertingService convertingService,
        ClosedGenericCallSiteTestCase.LocalKind kind)
    {
        return Local(kind);

        ClosedGenericCallSiteTestCase.VariableKind Local<TLocal>(TLocal value)
        {
            return convertingService.Convert<TLocal, ClosedGenericCallSiteTestCase.VariableKind>(value);
        }
    }
}

/// <summary>A type parameter of the type rather than of the method, closed by the type the method is called on.</summary>
public class KindMapper<TKind>
{
    private readonly IConvertingService _convertingService;

    public KindMapper(IConvertingService convertingService)
    {
        _convertingService = convertingService;
    }

    public ClosedGenericCallSiteTestCase.VariableKind Map(TKind kind)
    {
        return _convertingService.Convert<TKind, ClosedGenericCallSiteTestCase.VariableKind>(kind);
    }
}

/// <summary>
/// Internal on purpose: the composition is compiled into the test assembly as well, and a public copy
/// there would make the extension call ambiguous in the compilation the generator sees (a type of the
/// same name shadows the referenced one, an extension method does not).
/// </summary>
internal static class KindExtensions
{
    public static ClosedGenericCallSiteTestCase.VariableKind ToVariableKind<TKind>(
        this TKind kind,
        IConvertingService convertingService)
    {
        return convertingService.Convert<TKind, ClosedGenericCallSiteTestCase.VariableKind>(kind);
    }
}

/// <summary>
/// One of the pairs <c>BuildKeyTree</c> is closed into, written by hand. It answers <c>QrCode</c>
/// with <c>Barcode</c>, which no generated converter would, so the test can tell whose answer it got.
/// </summary>
public class PrintKindConverter
    : IConverter<ClosedGenericCallSiteTestCase.PrintVariable.PrintKind, ClosedGenericCallSiteTestCase.VariableKind>
{
    public ClosedGenericCallSiteTestCase.VariableKind Convert(ClosedGenericCallSiteTestCase.PrintVariable.PrintKind from)
    {
        return from switch
        {
            ClosedGenericCallSiteTestCase.PrintVariable.PrintKind.Variable => ClosedGenericCallSiteTestCase.VariableKind.Variable,
            ClosedGenericCallSiteTestCase.PrintVariable.PrintKind.QrCode => ClosedGenericCallSiteTestCase.VariableKind.Barcode,
            ClosedGenericCallSiteTestCase.PrintVariable.PrintKind.Barcode => ClosedGenericCallSiteTestCase.VariableKind.Barcode,
            ClosedGenericCallSiteTestCase.PrintVariable.PrintKind.Template => ClosedGenericCallSiteTestCase.VariableKind.Template,
            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
    }
}
