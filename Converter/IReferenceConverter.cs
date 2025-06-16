namespace Majipro.Converter;

public interface IReferenceConverter<in TFrom, TTo> : IConverter<TFrom, TTo>
{
    TTo Convert(TFrom from, TTo to);
}