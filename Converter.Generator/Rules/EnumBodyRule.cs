using System;
using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Generator.Diagnostics;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// Two enums, written as the switch a hand written converter would write: one case per source
/// member, returning the target member of the same name. Matching is by name only, so the two sides
/// are free to number their members differently - which is the whole reason the conversion is not
/// simply a cast.
/// </summary>
/// <remarks>
/// A source member the target does not have is the one thing this rule refuses: there is no
/// deterministic answer for it, and picking one would be a guess that compiles. So the rule claims
/// the pair - it is an enum conversion, no later rule would do better - and then throws
/// <see cref="ConversionException"/>, which stops the generation and fails the build on
/// <see cref="ConverterDiagnostics.EnumMemberWithoutCounterpart"/>. The other direction is fine:
/// target members nothing maps to are simply never returned.
///
/// An enum the caller already has is a value, not an instance to fill, so this conversion writes no
/// fill and the generated class stays a plain <c>IConverter</c>.
/// </remarks>
internal sealed class EnumBodyRule : IConverterBodyRule
{
    public IEnumerable<ConverterBody> Body(ConverterContext context)
    {
        if (context.Source.TypeKind != TypeKind.Enum ||
            context.Target.TypeKind != TypeKind.Enum ||
            context.Target.IsVisibleToGeneratedCode() == false)
        {
            yield break;
        }

        // Deciding is over, everything below happens because this rule won.
        yield return new ConverterBody(GetSwitch(context));
    }

    private static StatementSyntax GetSwitch(ConverterContext context)
    {
        var members = GetMappedMembers(context);

        var sections = members
            .Select(m => GetCase(context, m))
            .Concat(new[] { GetUnknownValueCase(context) });

        return SwitchStatement(context.SourceAccess)
            .WithSections(List(sections));
    }

    /// <summary>
    /// One source member per distinct value - two names for the same number would be the same
    /// <c>case</c> label twice - after every one of them has been found in the target.
    /// </summary>
    private static IEnumerable<IFieldSymbol> GetMappedMembers(ConverterContext context)
    {
        var target = new HashSet<string>(
            context.Target.GetEnumMembers().Select(m => m.Name),
            StringComparer.Ordinal);

        var source = context.Source.GetEnumMembers();
        var missing = source.Where(m => target.Contains(m.Name) == false).ToList();

        if (missing.Count > 0)
        {
            throw new ConversionException(
                ConverterDiagnostics.EnumMemberWithoutCounterpart,
                context.Source.ToReadableName(),
                context.Target.ToReadableName(),
                string.Join(", ", missing.Select(m => m.Name)));
        }

        var values = new HashSet<object>();

        return source.Where(m => m.ConstantValue != null && values.Add(m.ConstantValue));
    }

    private static SwitchSectionSyntax GetCase(ConverterContext context, IFieldSymbol member)
    {
        return SwitchSection()
            .WithLabels(
                SingletonList<SwitchLabelSyntax>(
                    CaseSwitchLabel(
                        ConverterSyntax.Member(ConverterSyntax.TypeName(context.Source), member.Name))))
            .WithStatements(
                SingletonList<StatementSyntax>(
                    ReturnStatement(
                        ConverterSyntax.Member(ConverterSyntax.TypeName(context.Target), member.Name))));
    }

    /// <summary>
    /// Every declared member is mapped, so this is reached only by a value that was cast into the
    /// enum without being one of its members. There is nothing to return for it, and the switch
    /// needs an arm that does not fall out of the method.
    /// </summary>
    private static SwitchSectionSyntax GetUnknownValueCase(ConverterContext context)
    {
        var message = "There is no '" + context.Target.ToReadableName() +
                      "' for this '" + context.Source.ToReadableName() + "'.";

        return SwitchSection()
            .WithLabels(
                SingletonList<SwitchLabelSyntax>(DefaultSwitchLabel()))
            .WithStatements(
                SingletonList<StatementSyntax>(
                    ThrowStatement(
                        ObjectCreationExpression(ConverterSyntax.TypeName(typeof(ArgumentOutOfRangeException)))
                            .WithArgumentList(
                                ArgumentList(
                                    SeparatedList(new[]
                                    {
                                        Argument(StringLiteral(ConverterSyntax.FromParameterName)),
                                        Argument(ConverterSyntax.From()),
                                        Argument(StringLiteral(message))
                                    }))))));
    }

    private static ExpressionSyntax StringLiteral(string value)
    {
        return LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(value));
    }
}
