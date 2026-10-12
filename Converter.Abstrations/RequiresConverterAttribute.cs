using System;

namespace Majipro.Converter.Abstrations;

/// <summary>
/// Calling the method this is put on asks the converting service for a converter between two of the
/// method's type parameters. The source generator reads it at every call site of the method - in any
/// assembly, since it is read off the method rather than out of its body - and requests the pair the
/// call closes those type parameters over, exactly as if <c>Convert&lt;From, To&gt;</c> had been
/// written there.
/// </summary>
/// <remarks>
/// Name the type parameters with <c>nameof</c>, which keeps a rename from silently pointing the
/// attribute at nothing: <c>[RequiresConverter(nameof(TFrom), nameof(TTo))]</c>. A method asking for
/// more than one converter - a dictionary asks for one for its keys and one for its values - carries
/// the attribute once per pair.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
public sealed class RequiresConverterAttribute : Attribute
{
    /// <param name="from">The name of the type parameter converted from.</param>
    /// <param name="to">The name of the type parameter converted to.</param>
    public RequiresConverterAttribute(string from, string to)
    {
        From = from;
        To = to;
    }

    /// <summary>The name of the type parameter converted from.</summary>
    public string From { get; }

    /// <summary>The name of the type parameter converted to.</summary>
    public string To { get; }
}
