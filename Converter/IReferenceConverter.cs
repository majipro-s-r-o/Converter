namespace Majipro.Converter;

/// <summary>
/// Reference Convertor between <see cref="TFrom"/> and <see cref="TTo"/>.
/// </summary>
/// <typeparam name="TFrom">Source type.</typeparam>
/// <typeparam name="TTo">Target type.</typeparam>
public interface IReferenceConverter<in TFrom, TTo> : IConverter<TFrom, TTo>
{
    /// <summary>
    /// Converts data from <see cref="TFrom"/> to existing instance of <see cref="TTo"/>.
    /// </summary>
    /// <typeparam name="TFrom">Source instance.</typeparam>
    /// <typeparam name="TTo">Target instance.</typeparam>
    /// <returns>Instance of <see cref="to"/> enhanced for data from <see cref="from"/>.</returns>
    TTo Convert(TFrom from, TTo to);
}