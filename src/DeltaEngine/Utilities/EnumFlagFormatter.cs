using System;

namespace Delta.Engine.Utilities;

internal static class EnumFlagFormatter<T> where T : unmanaged, Enum
{
    public static string Format(T value)
    {
        var valueToName = EnumBakedNames<T>.valueToName;
        return EnumFlagNameBuilder<T>.Build(value, valueToName);
    }
}
