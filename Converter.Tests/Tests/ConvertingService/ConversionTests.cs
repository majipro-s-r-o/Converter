using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class ConversionTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoObjectsThenConvertData()
    {
        // Arrange
        var expected = new Source
        {
            Integer = 1
        };

        // Act
        var actual = ConvertingService
            .Convert<Source, Target>(expected);
        
        // Assert
        Assert.AreEqual(expected.Integer, actual.Integer);
    }
}