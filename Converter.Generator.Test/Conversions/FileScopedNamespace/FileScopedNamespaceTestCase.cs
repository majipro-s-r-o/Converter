namespace Majipro.Converter.Generator.Test.Conversions.FileScopedNamespace;

/// <summary>
/// Unlike the other test cases these types are declared straight in a file scoped namespace
/// instead of being nested in a test case class.
/// </summary>
public class FileScopedNamespaceFrom
{
    public string Property { get; init; }
}

public class FileScopedNamespaceTo
{
    public string Property { get; init; }
}
