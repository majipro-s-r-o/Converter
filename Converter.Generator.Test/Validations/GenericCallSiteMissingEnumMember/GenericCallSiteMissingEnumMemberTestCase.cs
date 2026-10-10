namespace Majipro.Converter.Generator.Test.Validations.GenericCallSiteMissingEnumMember;

public class GenericCallSiteMissingEnumMemberTestCase
{
    /// <summary>
    /// The target has no <c>Barcode</c>, and the pair is never written down anywhere - it is what a
    /// generic call site is closed into by the method calling it.
    /// </summary>
    public enum Source
    {
        Variable,
        Barcode
    }

    public enum Target
    {
        Node,
        Variable
    }
}
