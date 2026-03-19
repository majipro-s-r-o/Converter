using System.Threading;
using System.Threading.Tasks;

namespace Majipro.Converter;

/// <summary>
/// Asynchronous converter between <see cref="TFrom"/> and <see cref="TTo"/>.
/// </summary>
/// <typeparam name="TFrom">Source type.</typeparam>
/// <typeparam name="TTo">Target type.</typeparam>
public interface IAsyncConverter<in TFrom, TTo>
{
  /// <summary>
  /// Converts from <see cref="TFrom"/> to <see cref="TTo"/> asynchronously.
  /// </summary>
  /// <param name="from">Source instance.</param>
  /// <param name="cancellationToken">Cancellation token.</param>
  /// <returns><see cref="Task{TTo}"/> returning converted instance.</returns>
  Task<TTo> ConvertAsync(TFrom from, CancellationToken cancellationToken);
}
