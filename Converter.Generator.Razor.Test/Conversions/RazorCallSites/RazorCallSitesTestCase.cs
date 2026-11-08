using System.Collections.Generic;

namespace Majipro.Converter.Generator.Razor.Test.Conversions.RazorCallSites;

/// <summary>
/// One source and a target of its own for every place in a Razor file a conversion can be written
/// at, so a converter being generated for a target says that very place was found.
/// </summary>
public class RazorCallSitesTestCase
{
    public class Person
    {
        public string Name { get; set; }
    }

    public abstract class NamedView
    {
        public string Name { get; set; }
    }

    /// <summary><c>@(...)</c> in markup.</summary>
    public class InlineView : NamedView
    {
    }

    /// <summary><c>@(...)</c> in the markup of a <c>@foreach</c>, converting its variable.</summary>
    public class LoopView : NamedView
    {
    }

    /// <summary><c>@Converter.Convert(...)</c>, inferred from a member of the code behind file.</summary>
    public class ImplicitView : NamedView
    {
    }

    /// <summary><c>@:</c> line in the <c>else</c> of an <c>@if</c>.</summary>
    public class ElseView : NamedView
    {
    }

    /// <summary>A <c>@{ ... }</c> block.</summary>
    public class BlockView : NamedView
    {
    }

    /// <summary>A member of the <c>@code</c> block.</summary>
    public class CodeView : NamedView
    {
    }

    /// <summary>Markup written in a method of the <c>@code</c> block.</summary>
    public class MarkupInCodeView : NamedView
    {
    }

    /// <summary>A <c>@&lt;p&gt;...&lt;/p&gt;</c> template in the <c>@code</c> block.</summary>
    public class TemplateView : NamedView
    {
    }

    /// <summary>A parameter of a component, set without any <c>@</c>.</summary>
    public class AttributeView : NamedView
    {
    }

    /// <summary>
    /// The <c>context</c> of a component's child content, the parameter of a delegate only Razor's
    /// own class knows the type of.
    /// </summary>
    public class ContextView : NamedView
    {
    }

    /// <summary>An <c>@onclick</c> handler.</summary>
    public class HandlerView : NamedView
    {
    }

    /// <summary>
    /// The code behind file converting through the service the <c>.razor</c> side <c>@inject</c>s -
    /// a property only Razor's own class declares.
    /// </summary>
    public class CodeBehindView : NamedView
    {
    }

    /// <summary>An <c>@code</c> member of a component that <c>@inject</c>s the service itself.</summary>
    public class InjectedCodeView : NamedView
    {
    }

    /// <summary>
    /// An <c>@code</c> method of the same component, the pair inferred from a member of its code
    /// behind file.
    /// </summary>
    public class InjectedInferredView : NamedView
    {
    }

    /// <summary>An <c>await ConvertAsync</c> in an <c>@code</c> lifecycle method of the same component.</summary>
    public class InjectedAsyncView : NamedView
    {
    }

    /// <summary>The code behind file of the same component, converting through its <c>@inject</c>.</summary>
    public class InjectedCodeBehindView : NamedView
    {
    }

    /// <summary>A <c>.cshtml</c> view converting its <c>@model</c>.</summary>
    public class ModelView : NamedView
    {
    }

    /// <summary>Written in a Razor comment, an HTML attribute and a C# string - nowhere it is called.</summary>
    public class IgnoredView : NamedView
    {
    }

    public class PeopleModel
    {
        public List<Person> People { get; set; }
    }
}
