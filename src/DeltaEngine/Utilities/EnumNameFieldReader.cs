using System;
using System.Collections.Generic;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal static class EnumNameFieldReader<T> where T : unmanaged, Enum
{
    public static List<EnumNameEntry<T>> Read()
    {
        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        List<EnumNameEntry<T>> entries = new(fields.Length);
        foreach (var field in fields)
        {
            if (EnumNameEntryReader<T>.TryRead(field, out var entry))
            {
                entries.Add(entry);
            }
        }

        return entries;
    }
}
