using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Conversions.InferredCallSites;

/// <summary>
/// Calls on the converting service that write no <c>&lt;TFrom, TTo&gt;</c> down, or write four of them.
/// Every one of them asks the container for a pair at runtime, so every one of them is a call site.
/// </summary>
public class InferredCallSitesComposition : TestCompositionBase
{
    /// <summary>The type arguments inferred from the source and the null fallback.</summary>
    public List<InferredCallSitesTestCase.ListTo> ConvertList(
        IConvertingService convertingService,
        List<InferredCallSitesTestCase.ListFrom> from)
    {
        return convertingService
            .Convert(from, () => new List<InferredCallSitesTestCase.ListTo>())
            .ToList();
    }

    public async Task<ISet<InferredCallSitesTestCase.SetTo>> ConvertSetAsync(
        IConvertingService convertingService,
        ISet<InferredCallSitesTestCase.SetFrom> from)
    {
        return await convertingService.ConvertAsync(from, () => new HashSet<InferredCallSitesTestCase.SetTo>());
    }

    /// <summary>A dictionary is two pairs: the keys and the values.</summary>
    public IDictionary<string, InferredCallSitesTestCase.DictionaryTo> ConvertDictionary(
        IConvertingService convertingService,
        IDictionary<Guid, InferredCallSitesTestCase.DictionaryFrom> from)
    {
        return convertingService
            .Convert<Guid, InferredCallSitesTestCase.DictionaryFrom, string, InferredCallSitesTestCase.DictionaryTo>(from);
    }
}