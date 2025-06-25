using System.Collections.Generic;
using System.Threading.Tasks;
using Majipro.Converter.Extensions;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.Extensions;

[TestClass]
public class ConvertingServiceExtensionsListTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new List<Source>
        {
            new Source
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(expected);
        
        // Assert
        CollectionAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new List<Source>
        {
            new Source
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(expected, () => new List<Target>());
        
        // Assert
        CollectionAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(null, () => new List<Target>());
        
        // Assert
        CollectionAssert.AreEqual(new List<Target>(), actual, new SourceTargetComparer());
    }
}