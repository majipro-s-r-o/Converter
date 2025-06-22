using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Majipro.Converter.Extensions;
using Majipro.Converter.Tests.Comparers;
using Majipro.Converter.Tests.Mocks;

namespace Majipro.Converter.Tests.Tests.Extensions;

[TestClass]
public sealed class ConvertingServiceExtensionsReadOnlyListTests : ConversionTestBase
{
    [ClassInitialize]
    public static async Task ThisTestClassInitializeAsync(TestContext context)
    {
        await ClassInitializeAsync();
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyListsWithoutFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new List<Source>
        {
            new Source
            {
                Integer = 1
            }
        } as IReadOnlyList<Source>;

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(expected);
        
        // Assert
        CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), new SourceTargetComparer());
    }
    
    [TestMethod]
    public void WhenConvertingBetweenTwoReadOnlyListsWithFallbackThenConvertAllItems()
    {
        // Arrange
        var expected = new List<Source>
        {
            new Source
            {
                Integer = 1
            }
        } as IReadOnlyList<Source>;

        // Act
        var actual = ConvertingService
            .ConvertExplicitly<Source, Target>(expected, () => new List<Target>());
        
        // Assert
        CollectionAssert.AreEqual(expected.ToList(), actual.ToList(), new SourceTargetComparer());
    }
}