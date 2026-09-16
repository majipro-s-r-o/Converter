namespace Majipro.Converter.Generator.Test.Validations.DisjointEnumMembers;

public class DisjointEnumMembersTestCase
{
    /// <summary>
    /// Neither side covers the other.
    /// </summary>
    public enum Left
    {
        Active = 0,
        Closed = 1
    }

    public enum Right
    {
        Unknown = 0,
        Active = 1
    }
}
