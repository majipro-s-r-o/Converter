namespace Majipro.Converter.Generator.Test.Conversions.AccessModifiers;

public class AccessModifiersTestCase
{
    /// <summary>
    /// Only the public property is seen by the generator.
    /// </summary>
    public class PublicPropertyFrom
    {
        public string PublicProperty { get; set; }

        private string PrivateProperty { get; set; }
    }

    public class PublicPropertyTo
    {
        public string PublicProperty { get; set; }

        private string PrivateProperty { get; set; }
    }

    /// <summary>
    /// Nothing the generator can see at all, the target is created empty.
    /// </summary>
    public class PrivatePropertyFrom
    {
        private string PrivateProperty { get; set; }
    }

    public class PrivatePropertyTo
    {
        private string PrivateProperty { get; set; }
    }

    /// <summary>
    /// An <c>init</c> only setter is written by an object initializer, so it is mapped.
    /// </summary>
    public class InitPropertyFrom
    {
        public string PublicProperty { get; init; }
    }

    public class InitPropertyTo
    {
        public string PublicProperty { get; init; }
    }

    /// <summary>
    /// The target property has no setter at all, so it stays untouched.
    /// </summary>
    public class GetOnlyPropertyFrom
    {
        public string PublicProperty { get; }
    }

    public class GetOnlyPropertyTo
    {
        public string PublicProperty { get; }
    }

    /// <summary>
    /// The target setter is private, so the generated code can not use it.
    /// </summary>
    public class PrivateSetterFrom
    {
        public string PublicProperty { get; private set; }
    }

    public class PrivateSetterTo
    {
        public string PublicProperty { get; private set; }
    }
}
