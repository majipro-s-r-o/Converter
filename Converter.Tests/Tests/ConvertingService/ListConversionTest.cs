using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class ListConversionTest : ConversionTestBase
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
            .Convert<Source, Target>(expected)
            .ToList();
        
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
            .Convert<Source, Target>(expected, () => new List<Target>())
            .ToList();
        
        // Assert
        CollectionAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoListsWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .Convert<Source, Target>(null, () => new List<Target>())
            .ToList();
        
        // Assert
        CollectionAssert.AreEqual(new List<Target>(), actual, new SourceTargetComparer());
    }
}