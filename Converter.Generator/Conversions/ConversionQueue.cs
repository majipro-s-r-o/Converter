using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Majipro.Converter.Generator.Conversions;

/// <summary>
/// The traveller. It hands out every pair that still needs a converter, exactly once, and keeps
/// doing so while it is being enumerated - whoever handles a pair is free to <see cref="Request"/>
/// more of them, and <see cref="Travel"/> picks those up in the same pass. That is what lets the
/// generator write a converter and discover the converters that one needs in a single act, instead
/// of analyzing everything first and generating afterwards.
/// </summary>
internal sealed class ConversionQueue
{
    private readonly Queue<ConversionPair> _pending = new Queue<ConversionPair>();
    private readonly HashSet<ConversionPair> _handled = new HashSet<ConversionPair>();

    public void Request(ConversionPair pair)
    {
        _pending.Enqueue(pair);
    }

    public void Request(ITypeSymbol from, ITypeSymbol to)
    {
        Request(new ConversionPair(from, to));
    }

    /// <summary>
    /// Takes a pair out of circulation without generating anything for it, which is how a converter
    /// somebody wrote by hand wins over a generated one.
    /// </summary>
    public void Suppress(ConversionPair pair)
    {
        _handled.Add(pair);
    }

    /// <summary>
    /// Every requested pair that has not been handled yet, in request order. Lazy on purpose: the
    /// consumer is expected to request more pairs while it is enumerating this.
    /// </summary>
    public IEnumerable<ConversionPair> Travel()
    {
        while (_pending.Count > 0)
        {
            var pair = _pending.Dequeue();

            // Marked as handled before it is handed out, which is also what stops a type that
            // contains itself from asking for its own converter forever.
            if (_handled.Add(pair) == false)
            {
                continue;
            }

            yield return pair;
        }
    }
}
