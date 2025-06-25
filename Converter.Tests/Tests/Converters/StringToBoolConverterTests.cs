using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToBoolConverterTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    [DataRow("true", true)]
    [DataRow(" true ", true)]
    [DataRow("True", true)]
    [DataRow("TRUE", true)]
    [DataRow("1", true)]
    [DataRow("false", false)]
    [DataRow(" false ", false)]
    [DataRow("False", false)]
    [DataRow("FALSE", false)]
    [DataRow("0", false)]
    [DataRow("random", false)]
    [DataRow("", false)]
    [DataRow(null, false)]
    public void WhenInputIsCanBeBoolThenReturnBoolOtherwiseFalse(string source, bool expected)
    {
        // Act
        var actual = ConvertingService.Convert<string, bool>(source);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
}