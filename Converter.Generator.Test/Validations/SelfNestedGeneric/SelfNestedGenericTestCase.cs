namespace Majipro.Converter.Generator.Test.Validations.SelfNestedGeneric;

public class SelfNestedGenericTestCase
{
    /// <summary>
    /// A generic type with a property of itself closed over itself. Converting one asks for the
    /// converter of the next one, which asks for the one after that, and every one of those is a
    /// pair nobody has handled yet - so there is no point at which the generation would find itself
    /// going in circles. What stops it is the bound on how deeply a name may nest.
    /// </summary>
    public class SourceNode<T>
    {
        public string Name { get; set; }

        public SourceNode<SourceNode<T>> Next { get; set; }
    }

    public class TargetNode<T>
    {
        public string Name { get; set; }

        public TargetNode<TargetNode<T>> Next { get; set; }
    }
}
