namespace Majipro.Converter;

/// <summary>
/// Basic Convertor between <see cref="TFrom"/> and <see cref="TTo"/>.
/// </summary>
/// <typeparam name="TFrom">Source type.</typeparam>
/// <typeparam name="TTo">Target type.</typeparam>
public interface IConverter<in TFrom, out TTo>
{
    /// <summary>
    /// Converts from <see cref="TFrom"/> to <see cref="TTo"/>.
    /// </summary>
    /// <param name="from">Source instance.</param>
    /// <returns>Converted instance.</returns>
    TTo Convert(TFrom from);
}