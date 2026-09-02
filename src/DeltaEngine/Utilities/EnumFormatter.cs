using System;
using System.Text;

namespace Delta.Engine.Utilities;

internal static class EnumFormatter<T> where T : unmanaged, Enum
{
    public static string Format(T value)
    {
        var valueToName = EnumBakedNames<T>.valueToName;
        const string Splitter = " | ";
        if (EnumBakedValues<T>.hasFlagsAttribute)
        {
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

        return valueToName[value];
    }
}
