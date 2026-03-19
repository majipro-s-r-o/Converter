using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
        var convertDelegate = GetConvertDelegate<TFrom, TTo>();

        return convertDelegate(from);
    }
    
    public ValueTask<TTo> ConvertAsync<TFrom, TTo>(TFrom from, CancellationToken cancellationToken = default)
    {
        var convertDelegate = GetConvertDelegateAsync<TFrom, TTo>();

        return convertDelegate(from, cancellationToken);
    }
    
    public TTo Convert<TFrom, TTo>(TFrom from, TTo to)
    {
        var convertDelegate = GetReferenceConvertDelegate<TFrom, TTo>();

        return convertDelegate(from, to);
    }

    public ValueTask<TTo> ConvertAsync<TFrom, TTo>(TFrom from, TTo to, CancellationToken cancellationToken = default)
    {
        var convertDelegate = GetReferenceConvertDelegateAsync<TFrom, TTo>();

        return convertDelegate(from, to, cancellationToken);
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

		var convertDelegate = GetConvertDelegate<TFrom, TTo>();

		return from.Select(convertDelegate).ToHashSet();
	}

	public async ValueTask<ISet<TTo>> ConvertAsync<TFrom, TTo>(ISet<TFrom> from, Func<ISet<TTo>> nullFallback, CancellationToken cancellationToken = default)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}

		if (from == null)
		{
			return nullFallback();
		}

		var convertDelegate = GetConvertDelegateAsync<TFrom, TTo>();
		var result = new HashSet<TTo>();

		foreach (var item in from)
		{
            var value = await convertDelegate(item, cancellationToken);
			result.Add(value);
		}

		return result;
	}

	public ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}

	public ValueTask<ISet<TTo>> ConvertAsync<TFrom, TTo>(ISet<TFrom> from, CancellationToken cancellationToken = default)
	{
		return ConvertAsync<TFrom, TTo>(from, () => null, cancellationToken);
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

        var convertDelegate = GetConvertDelegate<TFrom, TTo>();

		return from.Select(convertDelegate).ToList();
	}

	public async ValueTask<IList<TTo>> ConvertAsync<TFrom, TTo>(IList<TFrom> from, Func<IList<TTo>> nullFallback, CancellationToken cancellationToken = default)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}

		if (from == null)
		{
			return nullFallback();
		}

		var convertDelegate = GetConvertDelegateAsync<TFrom, TTo>();
		var result = new List<TTo>();

		foreach (var item in from)
		{
            var value = await convertDelegate(item, cancellationToken);
			result.Add(value);
		}

		return result;
	}

	public IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}

	public ValueTask<IList<TTo>> ConvertAsync<TFrom, TTo>(IList<TFrom> from, CancellationToken cancellationToken = default)
	{
		return ConvertAsync<TFrom, TTo>(from, () => null, cancellationToken);
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

        var convertDelegate = GetConvertDelegate<TFrom, TTo>();

		return from.Select(convertDelegate);
	}

	public async ValueTask<IEnumerable<TTo>> ConvertAsync<TFrom, TTo>(IEnumerable<TFrom> from, Func<IEnumerable<TTo>> nullFallback, CancellationToken cancellationToken = default)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}

		if (from == null)
		{
			return nullFallback();
		}

		var convertDelegate = GetConvertDelegateAsync<TFrom, TTo>();
		var result = new List<TTo>();

		foreach (var item in from)
		{
            var value = await convertDelegate(item, cancellationToken);
			result.Add(value);
		}

		return result;
	}

	public IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from)
	{
		return Convert<TFrom, TTo>(from, () => null);
	}

	public ValueTask<IEnumerable<TTo>> ConvertAsync<TFrom, TTo>(IEnumerable<TFrom> from, CancellationToken cancellationToken = default)
	{
		return ConvertAsync<TFrom, TTo>(from, () => null, cancellationToken);
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

        var convertDelegateKey = GetConvertDelegate<TFromKey, TToKey>();
        var convertDelegateValue = GetConvertDelegate<TFromValue, TToValue>();

		return from.ToDictionary(kvp => convertDelegateKey(kvp.Key), kvp => convertDelegateValue(kvp.Value));
	}

	public async ValueTask<IDictionary<TToKey, TToValue>> ConvertAsync<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from, Func<IDictionary<TToKey, TToValue>> nullFallback, CancellationToken cancellationToken = default)
	{
		if (nullFallback == null)
		{
			throw new ArgumentNullException(nameof(nullFallback));
		}

		if (from == null)
		{
			return nullFallback();
		}

		var convertDelegateKey = GetConvertDelegateAsync<TFromKey, TToKey>();
		var convertDelegateValue = GetConvertDelegateAsync<TFromValue, TToValue>();
		var result = new Dictionary<TToKey, TToValue>();

		foreach (var kvp in from)
		{
			var key = await convertDelegateKey(kvp.Key, cancellationToken);
			var value = await convertDelegateValue(kvp.Value, cancellationToken);
			result.Add(key, value);
		}

		return result;
	}

	public IDictionary<TToKey, TToValue> Convert<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from)
	{
		return Convert<TFromKey, TFromValue, TToKey, TToValue>(from, () => null);
	}

	public ValueTask<IDictionary<TToKey, TToValue>> ConvertAsync<TFromKey, TFromValue, TToKey, TToValue>(IDictionary<TFromKey, TFromValue> from, CancellationToken cancellationToken = default)
	{
		return ConvertAsync<TFromKey, TFromValue, TToKey, TToValue>(from, () => null, cancellationToken);
	}
    
    private Func<TFrom, TTo> GetConvertDelegate<TFrom, TTo>()
    {
        if (typeof(TFrom) == typeof(TTo))
        {
            return from => (TTo) (object) from;
        }
        
        var converter = _serviceProvider.GetService<IConverter<TFrom, TTo>>();

        if (converter != null)
        {
            return converter.Convert;
        }
        
        var asyncConverter = _serviceProvider.GetService<IAsyncConverter<TFrom, TTo>>();
        
        if (asyncConverter != null)
        {
            return from => Task
                .Run(() => asyncConverter.ConvertAsync(from, CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }
        
        throw new Exception($"There is no conversion from '{typeof(TFrom)}' to '{typeof(TTo)}'");
    }
    
    private Func<TFrom, CancellationToken, ValueTask<TTo>> GetConvertDelegateAsync<TFrom, TTo>()
    {
        if (typeof(TFrom) == typeof(TTo))
        {
            return (from, _) => new ValueTask<TTo>((TTo)(object)from);
        }
    
        var converter = _serviceProvider.GetService<IConverter<TFrom, TTo>>();
    
        if (converter != null)
        {
            return (from, _) => new ValueTask<TTo>(converter.Convert(from));
        }
    
        var asyncConverter = _serviceProvider.GetService<IAsyncConverter<TFrom, TTo>>();
    
        if (asyncConverter != null)
        {
            return (from, ct) => new ValueTask<TTo>(asyncConverter.ConvertAsync(from, ct));
        }
    
        throw new Exception($"There is no conversion from '{typeof(TFrom)}' to '{typeof(TTo)}'");
    }
    
    private Func<TFrom, TTo, TTo> GetReferenceConvertDelegate<TFrom, TTo>()
    {
        if (typeof(TFrom) == typeof(TTo))
        {
            return (from, _) => (TTo) (object) from;
        }
        
        var converter = _serviceProvider.GetService<IReferenceConverter<TFrom, TTo>>();

        if (converter != null)
        {
            return converter.Convert;
        }
        
        var asyncConverter = _serviceProvider.GetService<IAsyncReferenceConverter<TFrom, TTo>>();
        
        if (asyncConverter != null)
        {
            return (from, to) => Task
                .Run(() => asyncConverter.ConvertAsync(from, to, CancellationToken.None))
                .GetAwaiter()
                .GetResult();
        }
        
        throw new Exception($"There is no conversion from '{typeof(TFrom)}' to '{typeof(TTo)}'");
    }
    
    private Func<TFrom, TTo, CancellationToken, ValueTask<TTo>> GetReferenceConvertDelegateAsync<TFrom, TTo>()
    {
        if (typeof(TFrom) == typeof(TTo))
        {
            return (from, _, _) => new ValueTask<TTo>((TTo)(object)from);
        }

        var converter = _serviceProvider.GetService<IReferenceConverter<TFrom, TTo>>();

        if (converter != null)
        {
            return (from, to, _) => new ValueTask<TTo>(converter.Convert(from, to));
        }

        var asyncConverter = _serviceProvider.GetService<IAsyncReferenceConverter<TFrom, TTo>>();

        if (asyncConverter != null)
        {
            return (from, to, ct) => new ValueTask<TTo>(asyncConverter.ConvertAsync(from, to, ct));
        }

        throw new Exception($"There is no conversion from '{typeof(TFrom)}' to '{typeof(TTo)}'");
    }
}
