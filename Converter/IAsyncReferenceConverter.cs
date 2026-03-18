using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter;

/// <summary>
/// Asynchronous reference Converter between <see cref="TFrom"/> and <see cref="TTo"/>.
/// </summary>
/// <typeparam name="TFrom">Source type.</typeparam>
/// <typeparam name="TTo">Target type.</typeparam>
public interface IAsyncReferenceConverter<in TFrom, TTo> : IAsyncConverter<TFrom, TTo>
{
  /// <summary>
  /// Converts data from <see cref="TFrom"/> to existing instance of <see cref="TTo"/>.
  /// </summary>
  /// <typeparam name="TFrom">Source instance.</typeparam>
  /// <typeparam name="TTo">Target instance.</typeparam>
  /// <param name="from">Source instance.</param>
  /// <param name="to">Target instance.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns><see cref="Task{TTo}"/> returning instance of <see cref="to"/> enhanced for data from <see cref="from"/>.</returns>
  Task<TTo> ConvertAsync(TFrom from, TTo to, CancellationToken cancellationToken);
}
