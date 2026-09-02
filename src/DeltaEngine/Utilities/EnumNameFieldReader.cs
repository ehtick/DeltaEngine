using System;
using System.Collections.Generic;
using System.Reflection;

namespace Delta.Engine.Utilities;

internal static class EnumNameFieldReader<T> where T : unmanaged, Enum
{
    public static List<EnumNameEntry<T>> Read()
    {
        var fields = typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        return EnumNameFieldCollection<T>.Read(fields);
    }
}
