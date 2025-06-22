using System.Threading.Tasks;

namespace Majipro.Converter.Tests.Tests.Converters;

[TestClass]
public sealed class StringToByteConverterTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    [DataRow("25", 25)]
    [DataRow(" 26 ", 26)]
    [DataRow("256", 0)]
    [DataRow("-1", 0)]
    [DataRow("0", 0)]
    [DataRow("random", 0)]
    [DataRow("", 0)]
    [DataRow(null, 0)]
    public void WhenInputIsCanBeByteThenReturnByteOtherwiseZero(string source, int expected)
    {
        // Act
        var actual = ConvertingService.Convert<string, byte>(source);
        
        // Assert
        Assert.AreEqual(expected, actual);
    }
}