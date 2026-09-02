using System;
using System.Collections.Generic;
using System.Text;

namespace Delta.Engine.Utilities;

internal static class EnumFlagNameAccumulator<T> where T : unmanaged, Enum
{
    public static void Append(T value, Dictionary<T, string> valueToName, StringBuilder builder)
    {
        const string Splitter = " | ";
        foreach (var item in EnumBakedValues<T>.values)
        {
            if (value.HasFlag(item))
            {
                builder.Append(valueToName[item]).Append(Splitter);
            }
        }

        builder.Length -= Splitter.Length;
    }
}
