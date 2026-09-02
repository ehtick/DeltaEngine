using System;
using System.Collections.Generic;

namespace Delta.Engine.Utilities;

internal static class EnumNameMapBuilder<T> where T : unmanaged, Enum
{
    public static Dictionary<T, string> Create()
    {
        var entries = EnumNameFieldReader<T>.Read();
        Dictionary<T, string> result = [];
        AddPreferredNames(result, entries);
        AddFallbackNames(result, entries);
        return result;
    }

    private static void AddPreferredNames(Dictionary<T, string> result, List<EnumNameEntry<T>> entries)
    {
        foreach (var entry in entries)
        {
            if (!entry.IsObsolete && !result.ContainsKey(entry.Value))
            {
                result[entry.Value] = entry.Name;
            }
        }
    }

    private static void AddFallbackNames(Dictionary<T, string> result, List<EnumNameEntry<T>> entries)
    {
        foreach (var entry in entries)
        {
            if (entry.IsObsolete && !result.ContainsKey(entry.Value))
            {
                result[entry.Value] = entry.Name;
            }
        }
    }
}
