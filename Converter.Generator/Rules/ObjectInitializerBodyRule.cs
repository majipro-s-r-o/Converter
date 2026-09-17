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
///
/// This is also the only shape that can fill a target the caller already holds, so it is the one
/// that makes the generated class an <c>IReferenceConverter</c>. The same values are written a
/// second time as assignments on that instance, which is what a hand written reference converter
/// does: the properties the generator fills are overwritten, everything else the instance carries -
/// its identity first of all - survives. Two shapes have no such instance and are left out of it,
/// see <see cref="CanFill"/>.
/// </remarks>
internal sealed class ObjectInitializerBodyRule : IConverterBodyRule
{
    private readonly IReadOnlyList<IPropertyValueRule> _valueRules;

    public ObjectInitializerBodyRule(IReadOnlyList<IPropertyValueRule> valueRules)
    {
        _valueRules = valueRules;
    }

    public IEnumerable<ConverterBody> Body(ConverterContext context)
    {
        if (context.Source.IsMappableSource() == false || context.Target.IsMappableTarget() == false)
        {
            yield break;
        }

        // Deciding is over, everything below happens because this rule won.
        yield return GetBody(context);
    }

    private ConverterBody GetBody(ConverterContext context)
    {
        var assignments = GetAssignments(context);
        var create = GetCreate(context, assignments);

        return CanFill(context, assignments)
            ? new ConverterBody(create, assignments.Select(GetFill).ToList())
            : new ConverterBody(create);
    }

    /// <summary>
    /// Whether <c>to.Property = value</c> is something the generated code is allowed to write for
    /// every property it fills. Two things say it is not. An <c>init</c> only setter can be written
    /// by an object initializer and never again, so the target can be created but not filled. And a
    /// value type has no instance the caller holds: a structure handed to a method is a copy of it,
    /// so filling one would enhance something nobody else can see, which is not what the interface
    /// promises. A <c>Nullable&lt;T&gt;</c> target falls out of the same check.
    /// </summary>
    private static bool CanFill(ConverterContext context, IReadOnlyList<PropertyAssignment> assignments)
    {
        return context.Pair.To.IsReferenceType &&
               assignments.All(a => a.Target.IsSettable());
    }

    private static StatementSyntax GetCreate(ConverterContext context, IReadOnlyList<PropertyAssignment> assignments)
    {
        return ReturnStatement(
            ObjectCreationExpression(ConverterSyntax.TypeName(context.Target))
                .WithInitializer(
                    InitializerExpression(
                        SyntaxKind.ObjectInitializerExpression,
                        SeparatedList(assignments.Select(GetInitializer)))));
    }

    /// <summary><c>Property = value</c>, inside the object initializer.</summary>
    private static ExpressionSyntax GetInitializer(PropertyAssignment assignment)
    {
        return AssignmentExpression(
            SyntaxKind.SimpleAssignmentExpression,
            IdentifierName(assignment.Target.Name),
            assignment.Value);
    }

    /// <summary><c>to.Property = value;</c>, on the instance the caller passed in.</summary>
    private static StatementSyntax GetFill(PropertyAssignment assignment)
    {
        return ExpressionStatement(
            AssignmentExpression(
                SyntaxKind.SimpleAssignmentExpression,
                ConverterSyntax.Member(ConverterSyntax.To(), assignment.Target.Name),
                assignment.Value));
    }

    /// <summary>
    /// The whole intelligence of the generator, as one walk: every writable target property, the
    /// readable source property of the same name, and the value rule that knows what to do with it.
    /// Every one of them has to be found, see the remarks on the class. The value is asked for once
    /// and written out by both methods, so there is no way for the two of them to say different
    /// things about the same property.
    /// </summary>
    private IReadOnlyList<PropertyAssignment> GetAssignments(ConverterContext context)
    {
        var assignments = new List<PropertyAssignment>();
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

            assignments.Add(new PropertyAssignment(target, value));
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

    /// <summary>One target property and the value it was found: the same fact both methods write.</summary>
    private readonly struct PropertyAssignment
    {
        public IPropertySymbol Target { get; }

        public ExpressionSyntax Value { get; }

        public PropertyAssignment(IPropertySymbol target, ExpressionSyntax value)
        {
            Target = target;
            Value = value;
        }
    }
}
