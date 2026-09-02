using System;

namespace Delta.Engine.Utilities;

internal readonly record struct EnumNameEntry<T>(T Value, string Name, bool IsObsolete)
    where T : unmanaged, Enum;
