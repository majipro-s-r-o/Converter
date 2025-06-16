using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Majipro.Converter;

internal sealed class ConvertingService : IConvertingService
{
	private readonly IServiceProvider _serviceProvider;

	public ConvertingService(IServiceProvider serviceProvider)
	{
		_serviceProvider = serviceProvider;
	}

	/// <summary>
	/// Converts from <see cref="TFrom"/> to <see cref="TTo"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public TTo Convert<TFrom, TTo>(TFrom from)
	{
		var service = GetConverter<TFrom, TTo>();

		return service.Convert(from);
	}
	
	/// <summary>
	/// Converts from <see cref="ISet{T}"/> to <see cref="ISet{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from, Func<ISet<TTo>> nullFallback)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}
	        
		if (from == null)
		{
			return nullFallback();
		}
	        
		var service = GetConverter<TFrom, TTo>();

		return from.Select(service.Convert).ToHashSet();
	}
	
	/// <summary>
	/// Converts from <see cref="ISet{TFrom}"/> to <see cref="ISet{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}
	
	/// <summary>
	/// Converts from <see cref="IList{TFrom}"/> to <see cref="IList{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from, Func<IList<TTo>> nullFallback)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}
	        
		if (from == null)
		{
			return nullFallback();
		}
	        
		var service = GetConverter<TFrom, TTo>();

		return from.Select(service.Convert).ToList();
	}
	
	/// <summary>
	/// Converts from <see cref="IList{TFrom}"/> to <see cref="IList{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}
	
	/// <summary>
	/// Converts from <see cref="IEnumerable{TFrom}"/> to <see cref="IEnumerable{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <param name="nullFallback">Delegate that defines behavior if the <see cref="from"/> is null.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from, Func<IEnumerable<TTo>> nullFallback)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}
	        
		if (from == null)
		{
			return nullFallback();
		}
	        
		var service = GetConverter<TFrom, TTo>();

		return from.Select(service.Convert);
	}
	
	/// <summary>
	/// Converts from <see cref="IEnumerable{TFrom}"/> to <see cref="IEnumerable{TTo}"/> using <see cref="IConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);       
	}

	/// <summary>
	/// Converts from <see cref="TFrom"/> to <see cref="TTo"/> using <see cref="IReferenceConverter{TFrom, TTo}"/> implementation.
	/// </summary>
	/// <param name="from">Source of conversion.</param>
	/// <param name="to">Target instance.</param>
	/// <typeparam name="TFrom">Source type.</typeparam>
	/// <typeparam name="TTo">Target type.</typeparam>
	/// <returns>Result of conversion.</returns>
	public TTo Convert<TFrom, TTo>(TFrom from, TTo to)
	{
		var service = _serviceProvider.GetRequiredService<IReferenceConverter<TFrom, TTo>>();

		return service.Convert(from, to);
	}

	private IConverter<TFrom, TTo> GetConverter<TFrom, TTo>()
	{
		var service = _serviceProvider.GetService<IConverter<TFrom, TTo>>();

		if (service != null)
		{
			return service;
		}

		service = _serviceProvider.GetService<IReferenceConverter<TFrom, TTo>>();

		if (service == null)
		{
			throw new Exception($"There is no conversion from '{typeof(TFrom)}' to '{typeof(TTo)}'");
		}
            
		return service;
	}
}