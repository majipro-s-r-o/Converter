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

    [TestMethod]
    public void WhenConvertingBetweenTwoObjectsUsingReferenceConverterThenConvertData()
    {
        // Arrange
        var source = new SourceRef
        {
            Integer = 1
        };

        // Act
        var actual = ConvertingService
            .Convert<SourceRef, TargetRef>(source);

        // Assert
        Assert.AreEqual(source.Integer, actual.Integer);
    }

    [TestMethod]
    public void WhenConvertingToExistingInstanceUsingReferenceConverterThenConvertAndPreserveData()
    {
        // Arrange
        var source = new SourceRef
        {
            Integer = 42
        };

        var existingTarget = new TargetRef
        {
            Integer = 0,
            Text = "preserved"
        };

        // Act
        var actual = ConvertingService
            .Convert<SourceRef, TargetRef>(source, existingTarget);

        // Assert
        Assert.AreEqual(source.Integer, actual.Integer);
        Assert.AreEqual("preserved", actual.Text);
    }

    [TestMethod]
    public async ValueTask WhenConvertingUsingAsyncOverloadBetweenTwoObjectsUsingReferenceConverterThenConvertData()
    {
        // Arrange
        var source = new SourceRef
        {
            Integer = 1
        };

        // Act
        var actual = await ConvertingService
            .ConvertAsync<SourceRef, TargetRef>(source);

        // Assert
        Assert.AreEqual(source.Integer, actual.Integer);
    }

    [TestMethod]
    public async ValueTask WhenConvertingToExistingInstanceUsingAsyncOverloadOfReferenceConverterThenConvertAndPreserveData()
    {
        // Arrange
        var source = new SourceRef
        {
            Integer = 42
        };

        var existingTarget = new TargetRef
        {
            Integer = 0,
            Text = "preserved"
        };

        // Act
        var actual = await ConvertingService
            .ConvertAsync<SourceRef, TargetRef>(source, existingTarget);

        // Assert
        Assert.AreEqual(source.Integer, actual.Integer);
        Assert.AreEqual("preserved", actual.Text);
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
