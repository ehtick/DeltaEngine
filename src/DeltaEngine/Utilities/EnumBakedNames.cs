using System;
using System.Collections.Generic;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal readonly struct EnumBakedNames<T> where T : unmanaged, Enum
{
    public static readonly Dictionary<T, string> valueToName = CreateValueToName();

    private static Dictionary<T, string> CreateValueToName()
    {
        Dictionary<T, string> result = [];
        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        List<(T Value, string Name, bool Obsolete)> entries = new(fields.Length);
        foreach (var field in fields)
        {
            if (field.GetValue(null) is not T value)
            {
                continue;
            }

            bool obsolete = field.IsDefined(typeof(ObsoleteAttribute), false);
            entries.Add((value, field.Name, obsolete));
            if (!obsolete && !result.ContainsKey(value))
            {
                result[value] = field.Name;
            }
        }

        foreach (var entry in entries)
        {
            if (entry.Obsolete && !result.ContainsKey(entry.Value))
            {
                result[entry.Value] = entry.Name;
            }
        }

        return result;
    }
}
