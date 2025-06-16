using System;
using System.Linq;

namespace Majipro.Converter.Extensions;

internal static class TypeExtensions
{
    internal static bool IsAssignable(this Type type, Type assignableFor)
    {
        if (type == null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (type.IsClass == false)
        {
            return false;
        }

        if (assignableFor.IsAssignableFrom(type))
        {
            return true;
        }

        if (assignableFor.IsGenericType == false)
        {
            return false;
        }

        return type
            .GetInterfaces()
            .Any(t =>
            {
                if (t.IsGenericType == false)
                {
                    return false;
                }

                var getGenericTypeDefinition = t.GetGenericTypeDefinition();

                return getGenericTypeDefinition == assignableFor.GetGenericTypeDefinition();
            });
    }
}