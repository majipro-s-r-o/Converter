namespace Majipro.Converter.Generator.Test.Conversions.PropertyOfTheSameType;

[TestClass]
public class PropertyOfTheSameTypeTest : ConversionTestBase<PropertyOfTheSameTypeComposition>
{
    public PropertyOfTheSameTypeTest()
        : base(@".\Conversions\PropertyOfTheSameType\PropertyOfTheSameTypeComposition.cs")
    {
    }

    [TestMethod]
    public void WhenGeneratorRunsThenThereAreNoDiagnostics()
    {
        AssertGeneratedWithoutDiagnostics();

        Assert.AreEqual(1, GeneratedSources.Count, "Exactly one converter is expected for one conversion.");
    }

    [TestMethod]
    public void WhenConvertingByConvertingServiceThenAllPropertiesOfTheSameTypeShouldBeTheSame()
    {
        AssertGeneratedWithoutDiagnostics();

        var convertingService = GetConvertingService();

        var from = GetFrom();

        var to = convertingService.Convert<PropertyOfTheSameTypeTestCase.From, PropertyOfTheSameTypeTestCase.To>(from);

        AssertConverted(from, to);
    }

    private static PropertyOfTheSameTypeTestCase.From GetFrom()
    {
        return new PropertyOfTheSameTypeTestCase.From
        {
            Decimal = 1000.5m,
            UnsingnedLong = 111,
            Long = -100,
            UnsignedInteger = 11,
            Integer = -10,
            Byte = 1,
            Bool = true,
            String = "hello",
            Char = 'A'
        };
    }

    private static void AssertConverted(PropertyOfTheSameTypeTestCase.From from, PropertyOfTheSameTypeTestCase.To to)
    {
        Assert.IsNotNull(to);

        Assert.AreEqual(from.Decimal, to.Decimal);
        Assert.AreEqual(from.UnsingnedLong, to.UnsingnedLong);
        Assert.AreEqual(from.Long, to.Long);
        Assert.AreEqual(from.UnsignedInteger, to.UnsignedInteger);
        Assert.AreEqual(from.Integer, to.Integer);
        Assert.AreEqual(from.Byte, to.Byte);
        Assert.AreEqual(from.Bool, to.Bool);
        Assert.AreEqual(from.String, to.String);
        Assert.AreEqual(from.Char, to.Char);
    }
}
