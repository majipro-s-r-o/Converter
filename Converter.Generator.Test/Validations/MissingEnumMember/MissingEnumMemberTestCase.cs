namespace Majipro.Converter.Generator.Test.Validations.MissingEnumMember;

public class MissingEnumMemberTestCase
{
    /// <summary>
    /// The target has no <c>Active</c>, so there is nothing deterministic to return for it.
    /// </summary>
    public enum Wide
    {
        Unknown = 0,
        Active = 1,
        Closed = 2
    }

    public enum Narrow
    {
        Unknown = 0,
        Closed = 2
    }
}
