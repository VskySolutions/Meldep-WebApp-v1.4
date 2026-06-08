using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

public static class ObjectCleaner
{
    private static readonly HashSet<object> Visited = new();

    public static void RemoveEmptyOrNullProperties(object obj)
    {
        if (obj == null || Visited.Contains(obj)) return;

        Visited.Add(obj);

        var type = obj.GetType();

        if (IsSystemType(type)) return;

        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead || !prop.CanWrite) continue;

            var value = prop.GetValue(obj);
            var propType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;

            if (value == null)
            {
                prop.SetValue(obj, null);
            }
            else if (value is string strVal && string.IsNullOrWhiteSpace(strVal))
            {
                prop.SetValue(obj, null);
            }
            else if (value is IEnumerable enumerableVal && !(value is string))
            {
                var list = enumerableVal.Cast<object>().ToList();

                foreach (var item in list)
                {
                    RemoveEmptyOrNullProperties(item);
                }

                if (!list.Any() || list.All(IsObjectEmpty))
                {
                    prop.SetValue(obj, null);
                }
            }
            else if (!IsSimpleType(propType))
            {
                RemoveEmptyOrNullProperties(value);
                if (IsObjectEmpty(value))
                {
                    prop.SetValue(obj, null);
                }
            }
        }
    }

    private static bool IsObjectEmpty(object obj)
    {
        if (obj == null) return true;

        var props = obj.GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead);

        foreach (var prop in props)
        {
            var val = prop.GetValue(obj);

            if (val == null) continue;
            if (val is string str && string.IsNullOrWhiteSpace(str)) continue;
            if (val is IEnumerable enumerable && !(val is string) && !enumerable.Cast<object>().Any()) continue;

            return false;
        }

        return true;
    }

    private static bool IsSimpleType(Type type)
    {
        return type.IsPrimitive
               || type.IsEnum
               || type.Equals(typeof(string))
               || type.Equals(typeof(DateTime))
               || type.Equals(typeof(decimal))
               || type.Equals(typeof(Guid));
    }

    private static bool IsSystemType(Type type)
    {
        return type.Namespace != null && type.Namespace.StartsWith("System");
    }
}
