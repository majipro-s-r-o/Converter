using System.Threading.Tasks;
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
    public void WhenConvertingBetweenTwoObjectsUsingConverterThenConvertData()
    {
        AssertConversion<Source, Target>();
    }

    [TestMethod]
    public void WhenConvertingBetweenTwoObjectsUsingAsyncConverterThenConvertData()
    { 
        AssertConversion<SourceAsync, TargetAsync>(); 
    }

    private void AssertConversion<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new TSource
        {
            Integer = 1
        };

        // Act
        var actual = ConvertingService
            .Convert<TSource, TTarget>(expected);

        // Assert
        Assert.AreEqual(expected.Integer, actual.Integer);
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoObjectsUsingConverterThenConvertData()
    {
        await AssertConversionAsync<Source, Target>();
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoObjectsUsingAsyncConverterThenConvertData()
    {
        await AssertConversionAsync<SourceAsync, TargetAsync>();
    }

    private async ValueTask AssertConversionAsync<TSource, TTarget>()
        where TSource : IHasInteger, new()
        where TTarget : IHasInteger
    {
        // Arrange
        var expected = new TSource
        {
            Integer = 1
        };

        // Act
        var actual = await ConvertingService
            .ConvertAsync<TSource, TTarget>(expected);

        // Assert
        Assert.AreEqual(expected.Integer, actual.Integer);
    }
}
