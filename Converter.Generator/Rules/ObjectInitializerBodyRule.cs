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
/// The ordinary conversion: a new target written as an object initializer. Properties the source
/// knows and the target does not are dropped, which loses nothing the target was ever going to
/// carry.
/// </summary>
/// <remarks>
/// The other direction is the one this rule refuses, the same way <see cref="EnumBodyRule"/>
/// refuses a member without a counterpart: a writable target property nothing can fill would be
/// left at its default, and a converter that silently returns a half built object is worse than no
/// converter at all. So the rule claims the pair - both sides are mappable, no later rule would do
/// better - and then throws <see cref="ConversionException"/>, which stops the generation and fails
/// the build on <see cref="ConverterDiagnostics.PropertyWithoutCounterpart"/>. Nothing can fill it
/// covers both reasons: the source has no property of that name, or it has one and no value rule
/// knows what to do with it. A property the object initializer could not write anyway - no setter,
/// or a setter that is not public - is not the generator's to fill and is never counted.
/// </remarks>
internal sealed class ObjectInitializerBodyRule : IConverterBodyRule
{
    private readonly IReadOnlyList<IPropertyValueRule> _valueRules;

    public ObjectInitializerBodyRule(IReadOnlyList<IPropertyValueRule> valueRules)
    {
        _valueRules = valueRules;
    }

    public IEnumerable<StatementSyntax> Body(ConverterContext context)
    {
        if (context.Source.IsMappableSource() == false || context.Target.IsMappableTarget() == false)
        {
            yield break;
        }

        // Deciding is over, everything below happens because this rule won.
        yield return ReturnStatement(
            ObjectCreationExpression(ConverterSyntax.TypeName(context.Target))
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SeparatedList(GetAssignments(context)))));
    }

    /// <summary>
    /// The whole intelligence of the generator, as one walk: every writable target property, the
    /// readable source property of the same name, and the value rule that knows what to do with it.
    /// Every one of them has to be found, see the remarks on the class.
    /// </summary>
    private IReadOnlyList<ExpressionSyntax> GetAssignments(ConverterContext context)
    {
        var assignments = new List<ExpressionSyntax>();
        var unfilled = new List<string>();

        var targets = context.Target
            .GetInstanceProperties()
            .Where(p => p.IsWritable());

        foreach (var target in targets)
        {
            var value = GetValue(context, target);

            if (value == null)
            {
                unfilled.Add(target.Name);

                continue;
            }

            assignments.Add(
                AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    IdentifierName(target.Name),
                    value));
        }

        if (unfilled.Count > 0)
        {
            throw new ConversionException(
                ConverterDiagnostics.PropertyWithoutCounterpart,
                context.Source.ToReadableName(),
                context.Target.ToReadableName(),
                string.Join(", ", unfilled));
        }

        return assignments;
    }

    private ExpressionSyntax? GetValue(ConverterContext context, IPropertySymbol target)
    {
        return (from source in context.SourceProperty(target.Name)
                from value in _valueRules.FirstMatch(r => r.Value(context, source, target, context.Read(source)))
                select value).FirstOrDefault();
    }
}
