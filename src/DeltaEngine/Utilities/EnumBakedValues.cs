using System;

namespace Delta.Engine.Utilities;

internal readonly struct EnumBakedValues<T> where T : struct, Enum
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
