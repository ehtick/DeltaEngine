using System;
using System.Collections.Generic;
using System.Text;

namespace Delta.Engine.Utilities;

internal static class EnumFlagNameBuilder<T> where T : unmanaged, Enum
{
    public static string Build(T value, Dictionary<T, string> valueToName)
    {
        const string Splitter = " | ";
        StringBuilder builder = new();
        foreach (var item in EnumBakedValues<T>.values)
        {
            if (value.HasFlag(item))
            {
                builder.Append(valueToName[item]).Append(Splitter);
            }
        }

        builder.Length -= Splitter.Length;
        return builder.ToString();
    }
}
