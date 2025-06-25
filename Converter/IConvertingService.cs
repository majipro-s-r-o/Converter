using System;
using System.Collections.Generic;

namespace Majipro.Converter;

public interface IConvertingService
{
    /// <summary>
    /// Converts from <see cref="TFrom"/> to <see cref="TTo"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    TTo Convert<TFrom, TTo>(TFrom from);

    /// <summary>
    /// Converts from <see cref="ISet{T}"/> to <see cref="ISet{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from, Func<ISet<TTo>> nullFallback);
    
    /// <summary>
    /// Converts from <see cref="ISet{TFrom}"/> to <see cref="ISet{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from);

    /// <summary>
    /// Converts from <see cref="IList{TFrom}"/> to <see cref="IList{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from, Func<IList<TTo>> nullFallback);
    
    /// <summary>
    /// Converts from <see cref="IList{TFrom}"/> to <see cref="IList{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from);

    /// <summary>
    /// Converts from <see cref="IEnumerable{TFrom}"/> to <see cref="IEnumerable{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from, Func<IEnumerable<TTo>> nullFallback);
    
    /// <summary>
    /// Converts from <see cref="IEnumerable{TFrom}"/> to <see cref="IEnumerable{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from);
    
    /// <summary>
    /// Converts from <see cref="IDictionary{TFromKey, TFromValue}"/> to <see cref="IDictionary{TToKey, TToValue}"/> using <see cref="IConverter{TFromKey, TToKey}"/> and <see cref="IConverter{TFromValue, TToValue}"/> implementations.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
    /// <typeparam name="TFromKey">Source key type.</typeparam>
    /// <typeparam name="TToKey">Target key type.</typeparam>
    /// <typeparam name="TFromValue">Source value type.</typeparam>
    /// <typeparam name="TToValue">Target value type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IDictionary<TToKey, TToValue> Convert<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from, Func<IDictionary<TToKey, TToValue>> nullFallback);

    /// <summary>
    /// Converts from <see cref="IDictionary{TFromKey, TFromValue}"/> to <see cref="IDictionary{TToKey, TToValue}"/> using <see cref="IConverter{TFromKey, TToKey}"/> and <see cref="IConverter{TFromValue, TToValue}"/> implementations.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <typeparam name="TFromKey">Source key type.</typeparam>
    /// <typeparam name="TToKey">Target key type.</typeparam>
    /// <typeparam name="TFromValue">Source value type.</typeparam>
    /// <typeparam name="TToValue">Target value type.</typeparam>
    /// <returns>Result of conversion.</returns>
    IDictionary<TToKey, TToValue> Convert<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from);

    /// <summary>
    /// Converts from <see cref="TFrom"/> to <see cref="TTo"/> using <see cref="IReferenceConverter{TFrom, TTo}"/> implementation.
    /// </summary>
    /// <param name="from">Source of conversion.</param>
    /// <param name="to">Target instance.</param>
    /// <typeparam name="TFrom">Source type.</typeparam>
    /// <typeparam name="TTo">Target type.</typeparam>
    /// <returns>Result of conversion.</returns>
    TTo Convert<TFrom, TTo>(TFrom from, TTo to);
}