using System;
using System.Collections.Generic;
using System.Text;

namespace Delta.Engine.Utilities;

internal static class EnumFlagNameBuilder<T> where T : unmanaged, Enum
{
    public static string Build(T value, Dictionary<T, string> valueToName)
    {
        StringBuilder builder = new();
        EnumFlagNameAccumulator<T>.Append(value, valueToName, builder);
        return builder.ToString();
    }
}
