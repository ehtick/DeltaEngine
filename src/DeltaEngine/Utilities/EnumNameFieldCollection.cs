using System;
using System.Collections.Generic;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal static class EnumNameFieldCollection<T> where T : unmanaged, Enum
{
    public static List<EnumNameEntry<T>> Read(FieldInfo[] fields)
    {
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
