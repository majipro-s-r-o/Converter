using System;
using System.Collections.Generic;
using System.Linq;

namespace Majipro.Converter.Generator.Extensions;

internal static class RuleExtensions
{
    /// <summary>
    /// The node of the first rule that produces one, or nothing when none of them does. Lazy, so
    /// the rules after the winning one are never asked - which is also why a rule must not do
    /// anything but decide until the moment it yields.
    /// </summary>
    internal static IEnumerable<TNode> FirstMatch<TRule, TNode>(
        this IEnumerable<TRule> rules,
        Func<TRule, IEnumerable<TNode>> apply)
    {
        return rules.SelectMany(apply).Take(1);
    }
}
