using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Generator.Test.Conversions.MarkedCallSites;

public class MarkedCallSitesTestCase
{
    public class ExplicitListFrom
    {
        public string Name { get; set; }
    }

    public class ExplicitListTo
    {
        public string Name { get; set; }
    }

    public class ExplicitArrayFrom
    {
        public string Name { get; set; }
    }

    public class ExplicitArrayTo
    {
        public string Name { get; set; }
    }

    public class LocalFrom
    {
        public string Name { get; set; }
    }

    public class LocalTo
    {
        public string Name { get; set; }
    }

    public class ReferencedFrom
    {
        public string Name { get; set; }
    }

    public class ReferencedTo
    {
        public string Name { get; set; }
    }

    public class NestedFrom
    {
        public string Name { get; set; }
    }

    public class NestedTo
    {
        public string Name { get; set; }
    }

    public class GenericFrom
    {
        public string Name { get; set; }
    }

    public class GenericTo
    {
        public string Name { get; set; }
    }

    public class ExtensionFrom
    {
        public string Name { get; set; }
    }

    public class ExtensionTo
    {
        public string Name { get; set; }
    }

    public class ContractFrom
    {
        public string Name { get; set; }
    }

    public class ContractTo
    {
        public string Name { get; set; }
    }

    public class OverrideFrom
    {
        public string Name { get; set; }
    }

    public class OverrideTo
    {
        public string Name { get; set; }
    }

    public class ClassLevelFrom
    {
        public string Name { get; set; }
    }

    public class ClassLevelTo
    {
        public string Name { get; set; }
    }

    public class LocalFunctionFrom
    {
        public string Name { get; set; }
    }

    public class LocalFunctionTo
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// A marked method in <b>another assembly</b>: this file is compiled into the test assembly only,
    /// which the in-memory compilation references, so the generator sees the call of it and never its
    /// body - the shape of a generic helper in a class library. The type parameters are declared in
    /// the opposite order to the one the attribute names them in, so the pair can only come out right
    /// when it is read off the attribute rather than off the positions.
    /// </summary>
    [RequiresConverter(nameof(TSource), nameof(TTarget))]
    public static TTarget Relay<TTarget, TSource>(IConvertingService convertingService, TSource from)
    {
        return convertingService.Convert<TSource, TTarget>(from);
    }

    /// <summary>
    /// A marked method declared in a nested type, which the walk collecting the names of marked
    /// methods has to descend into.
    /// </summary>
    public static class Helpers
    {
        [RequiresConverter(nameof(TFrom), nameof(TTo))]
        public static TTo NestedRelay<TFrom, TTo>(IConvertingService convertingService, TFrom from)
        {
            return convertingService.Convert<TFrom, TTo>(from);
        }
    }
}
