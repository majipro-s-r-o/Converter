using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToLongConverterTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    [DataRow("25", 25)]
    [DataRow(" 26 ", 26)]
    [DataRow("9223372036854775808", 0)]
    [DataRow("-9223372036854775808", 0)]
    [DataRow("0", 0)]
    [DataRow("random", 0)]
    [DataRow("", 0)]
    [DataRow(null, 0)]
    public void WhenInputIsCanBeLongThenReturnLongOtherwiseZero(string source, int expected)
    {
        // Act
        var actual = ConvertingService.Convert<string, byte>(source);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
}