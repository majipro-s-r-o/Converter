using System;
using System.Collections.Generic;

namespace Majipro.Converter;

public interface IConvertingService
{
    TTo Convert<TFrom, TTo>(TFrom from);

    ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from, Func<ISet<TTo>> nullFallback);
    ISet<TTo> Convert<TFrom, TTo>(ISet<TFrom> from);

    IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from, Func<IList<TTo>> nullFallback);
    IList<TTo> Convert<TFrom, TTo>(IList<TFrom> from);

    IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from, Func<IEnumerable<TTo>> nullFallback);
    IEnumerable<TTo> Convert<TFrom, TTo>(IEnumerable<TFrom> from);

    TTo Convert<TFrom, TTo>(TFrom from, TTo to);
}