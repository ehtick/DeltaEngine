using System;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal static class EnumNameEntryReader<T> where T : unmanaged, Enum
{
    public static bool TryRead(FieldInfo field, out EnumNameEntry<T> entry)
    {
        if (field.GetValue(null) is not T value)
        {
            entry = default;
            return false;
        }

        entry = new(value, field.Name, field.IsDefined(typeof(ObsoleteAttribute), false));
        return true;
    }
}
