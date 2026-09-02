using System;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal readonly struct EnumBakedNames<T> where T : unmanaged, Enum
{
    public static readonly Dictionary<T, string> valueToName = EnumNameMapBuilder<T>.Create();
}
