using System.Collections.Generic;
using System.Linq;
using Majipro.Converter.Generator.Extensions;
using Majipro.Converter.Generator.Generating;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Majipro.Converter.Generator.Rules;

/// <summary>
/// The ordinary conversion: a new target written as an object initializer. A target property with
/// no source counterpart, or one no value rule claims, is left out; a conversion where that is all
/// of them still gets a converter that returns an empty instance.
/// </summary>
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

        yield return ReturnStatement(
            ObjectCreationExpression(ConverterSyntax.TypeName(context.Target))
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SeparatedList(GetAssignments(context)))));
    }

    /// <summary>
    /// The whole intelligence of the generator, as one walk: every writable target property that
    /// has a readable source property of the same name, and a value rule that knows what to do
    /// with it.
    /// </summary>
    private IEnumerable<ExpressionSyntax> GetAssignments(ConverterContext context)
    {
        return from target in context.Target.GetInstanceProperties()
               where target.IsWritable()
               from source in context.SourceProperty(target.Name)
               from value in _valueRules.FirstMatch(r => r.Value(context, source, target, context.Read(source)))
               select (ExpressionSyntax)AssignmentExpression(
                   SyntaxKind.SimpleAssignmentExpression,
                   IdentifierName(target.Name),
                   value);
    }
}
