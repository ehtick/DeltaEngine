using System;

namespace Delta.Engine.Utilities;

internal static class EnumFormatter<T> where T : unmanaged, Enum
{
    public static string Format(T value)
        => EnumBakedValues<T>.hasFlagsAttribute
            ? EnumFlagFormatter<T>.Format(value)
            : EnumBakedNames<T>.valueToName[value];
}
