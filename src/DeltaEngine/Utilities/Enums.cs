using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Delta.Engine.Utilities;

internal static class Enums
{
    private readonly struct EnumBakedValues<T> where T : struct, Enum
    {
        public static readonly T[] values = CreateValues();
        public static readonly int count = values.Length;
        public static readonly bool hasFlagsAttribute = typeof(T).IsDefined(typeof(FlagsAttribute), false);

        private static T[] CreateValues()
        {
            var result = Enum.GetValues<T>();
            int count = SpanExtensions.Distinct<T>(result);
            return result[..count];
        }
    }

    private readonly struct EnumBakedNames<T> where T : unmanaged, Enum
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

    public static int GetCount<T>() where T : struct, Enum => EnumBakedValues<T>.count;
    public static ReadOnlySpan<T> GetValues<T>() where T : struct, Enum => EnumBakedValues<T>.values;

    /// <summary>
    /// Returns string representing current enum or enum flags.
    /// All elements are distinct by its undrelying type e.g. int, long, uint.
    /// If element is marked as obsolete, it's name will not be used
    /// except cases when replacement not found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString<T>(T value) where T : unmanaged, Enum
    {
        var valueToName = EnumBakedNames<T>.valueToName;
        const string Splitter = " | ";
        if (EnumBakedValues<T>.hasFlagsAttribute)
        {
            StringBuilder sb = new();

            foreach (var item in EnumBakedValues<T>.values)
            {
                if (value.HasFlag(item))
                {
                    sb.Append(valueToName[item]).Append(Splitter);
                }
            }

            sb.Length -= Splitter.Length;

            return sb.ToString();
        }
        else
        {
            return valueToName[value];
        }
    }
}
