using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.ConvertingService;

[TestClass]
public sealed class SetConversionTest : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new HashSet<Source>
        {
            new Source
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<Source, Target>(expected)
            .ToHashSet();
        
        // Assert
        HashSetAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetsWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new HashSet<Source>
        {
            new Source
            {
                Integer = 1
            }
        };

        // Act
        var actual = ConvertingService
            .Convert<Source, Target>(expected, () => new HashSet<Target>())
            .ToHashSet();
        
        // Assert
        HashSetAssert.AreEqual(expected, actual, new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoHashSetWithFallbackThenUseFallbackIfInputIsNull()
    {
        // Act
        var actual = ConvertingService
            .Convert<Source, Target>(null, () => new List<Target>())
            .ToHashSet();
        
        // Assert
        HashSetAssert.AreEqual(new HashSet<Target>(), actual, new SourceTargetComparer());
    }
}