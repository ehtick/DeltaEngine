using System;
using System.Runtime.CompilerServices;

namespace Delta.Engine.EditorLib.Scripting;

internal static class AccessorValuePathReader
{
    public static unsafe TValue Read<TValue>(IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(type);
        foreach (var fieldName in path)
        {
            var accessor = container.AllAccessors[type];
            address = accessor.GetFieldPtr(address, fieldName);
            type = accessor.GetFieldType(fieldName);
        }

        if (typeof(TValue) == type)
        {
            return Unsafe.AsRef<TValue>(address.ToPointer());
        }

        throw new InvalidOperationException($"Accessor type {type} does not match requested value type {typeof(TValue)}.");
    }
}
