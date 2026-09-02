using System;
using System.Linq;
using System.Reflection;

internal static class AccessorNameFormatter
{
    public static string GetSetMethodName(FieldInfo field) => "GetSet_" + field.Name;

    public static string GetFormattedName(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericArguments = type.GetGenericArguments()
            .Select(GetFormattedName)
            .Aggregate((left, right) => $"{left}, {right}");
        const string marker = "`";
        return $"{type.Name[..type.Name.IndexOf(marker, StringComparison.Ordinal)]}<{genericArguments}>";
    }

    public static string GetAccessorName(Type type) => $"{GetAccessorNameArguments(type)}Accessor";

    private static string GetAccessorNameArguments(Type type)
    {
        if (!type.IsGenericType)
        {
            return type.Name;
        }

        var genericArguments = type.GetGenericArguments()
            .Select(GetFormattedName)
            .Aggregate((left, right) => $"{left}_{right}");
        const string marker = "`";
        return $"{type.Name[..type.Name.IndexOf(marker, StringComparison.Ordinal)]}__{genericArguments}__";
    }
}
