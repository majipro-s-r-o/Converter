using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Majipro.Converter.Abstrations;
using Majipro.Converter.Extensions;

namespace Majipro.Converter.Tests.Tests;

/// <summary>
/// The source generator knows what a method of the converting service asks for only from its
/// <see cref="RequiresConverterAttribute"/>. An overload without one compiles, and its pair is then
/// never generated nor reported - the call throws at runtime about a conversion nobody wrote. The
/// generator suite calls only a few of the overloads, so this is what guards all of them, including
/// the ones added later.
/// </summary>
[TestClass]
public sealed class RequiresConverterAnnotationTests
{
    [TestMethod]
    public void WhenAMethodOfTheConvertingServiceIsDeclaredThenItRequiresItsPairs()
    {
        AssertRequiresItsPairs(typeof(IConvertingService).GetMethods());
    }

    [TestMethod]
    public void WhenAConvertExplicitlyIsDeclaredThenItRequiresItsPair()
    {
        AssertRequiresItsPairs(typeof(ConvertingServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Where(m => m.Name == nameof(ConvertingServiceExtensions.ConvertExplicitly)));
    }

    private static void AssertRequiresItsPairs(IEnumerable<MethodInfo> methods)
    {
        var checkedMethods = 0;
        var wrong = new List<string>();

        foreach (var method in methods)
        {
            checkedMethods++;

            var expected = GetExpectedPairs(method);
            var actual = method
                .GetCustomAttributes<RequiresConverterAttribute>()
                .Select(a => a.From + " -> " + a.To)
                .OrderBy(p => p, StringComparer.Ordinal)
                .ToList();

            if (expected.SequenceEqual(actual) == false)
            {
                wrong.Add($"{method}: expected [{string.Join(", ", expected)}], found [{string.Join(", ", actual)}]");
            }
        }

        Assert.IsGreaterThan(0, checkedMethods, "No method was checked at all.");
        Assert.IsEmpty(wrong, "Methods not saying what they require:" + Environment.NewLine + string.Join(Environment.NewLine, wrong));
    }

    /// <summary>
    /// A value or a collection converts <c>TFrom</c> into <c>TTo</c>. A dictionary converts its keys
    /// and its values, each a pair of its own.
    /// </summary>
    private static List<string> GetExpectedPairs(MethodInfo method)
    {
        var names = method.GetGenericArguments().Select(t => t.Name).ToList();

        var expected = names.Count == 4
            ? new List<string> { names[0] + " -> " + names[2], names[1] + " -> " + names[3] }
            : new List<string> { names[0] + " -> " + names[1] };

        return expected.OrderBy(p => p, StringComparer.Ordinal).ToList();
    }
}
