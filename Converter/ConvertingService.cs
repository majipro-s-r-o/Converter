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
	
	public TTo Convert<TFrom, TTo>(TFrom from)
	{
		var service = GetConverter<TFrom, TTo>();

		return service.Convert(from);
	}
	
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
	
	public ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}
	
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
	
	public IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}
	
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
	
	public IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);       
	}
	
	public IDictionary<TToKey, TToValue> Convert<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from, Func<IDictionary<TToKey, TToValue>> nullFallback)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}
	        
		if (from == null)
		{
			return nullFallback();
		}

		var convertKey = GetConversionFunction<TFromKey, TToKey>();
		var convertValue = GetConversionFunction<TFromValue, TToValue>();
		
		return from.ToDictionary(kvp => convertKey(kvp.Key), kvp => convertValue(kvp.Value));
	}
	
	public IDictionary<TToKey, TToValue> Convert<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from)
	{
		return Convert<TFromKey, TFromValue, TToKey, TToValue>(from, () => null);
	}
	
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

	private Func<TFrom, TTo> GetConversionFunction<TFrom, TTo>()
	{
		return typeof(TFrom) == typeof(TTo)
			? key => (TTo)(object)key
			: GetConverter<TFrom, TTo>().Convert;
	}
}