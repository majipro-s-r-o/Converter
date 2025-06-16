namespace Majipro.Converter;

public interface IConverter<in TFrom, out TTo>
{
    TTo Convert(TFrom from);
}