using Majipro.Converter.Abstrations;

namespace Majipro.Converter.Generator.Test.Validations.UnknownRequiredTypeParameter;

public class UnknownRequiredTypeParameterTestCase
{
    public class From
    {
        public string Name { get; set; }
    }

    public class To
    {
        public string Name { get; set; }
    }

    /// <summary>
    /// The attribute is written with a string instead of <c>nameof</c>, and the string names no type
    /// parameter of the method - nothing but the generator can notice.
    /// </summary>
    [RequiresConverter(nameof(TFrom), "TTarget")]
    public static TTo RelayWithTypo<TFrom, TTo>(IConvertingService convertingService, TFrom from)
    {
        return convertingService.Convert<TFrom, TTo>(from);
    }
}
