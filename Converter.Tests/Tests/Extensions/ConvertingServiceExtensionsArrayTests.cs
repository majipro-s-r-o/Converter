using System;
using System.Threading.Tasks;
using Majipro.Converter.Extensions;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.Extensions;

[TestClass]
public sealed class ConvertingServiceExtensionsArrayTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoArraysWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Source[]
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
    public void WhenConvertingBetweenTwoArraysWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new Source[]
        {
            new Source
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(expected, Array.Empty<Target>);
        
        // Assert
        CollectionAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoArraysWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(null, Array.Empty<Target>);
        
        // Assert
        CollectionAssert.AreEqual(Array.Empty<Target>(), actual, new SourceTargetComparer());
    }
}