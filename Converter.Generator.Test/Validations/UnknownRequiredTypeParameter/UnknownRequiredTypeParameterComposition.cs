using Majipro.Converter.Abstrations;
using Majipro.Converter.Generator.Test;

namespace Majipro.Converter.Generator.Test.Validations.UnknownRequiredTypeParameter;

/// <summary>
/// A call of a marked method whose attribute names a type parameter the method does not have. It is a
/// compilation of its own, because the error ends the whole generation pass.
/// </summary>
public class UnknownRequiredTypeParameterComposition : TestCompositionBase
{
    public UnknownRequiredTypeParameterTestCase.To ConvertWithTypo(
        IConvertingService convertingService,
        UnknownRequiredTypeParameterTestCase.From from)
    {
        return UnknownRequiredTypeParameterTestCase.RelayWithTypo<UnknownRequiredTypeParameterTestCase.From, UnknownRequiredTypeParameterTestCase.To>(
            convertingService,
            from);
    }
}
