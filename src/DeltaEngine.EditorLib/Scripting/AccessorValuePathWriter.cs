using System;
using System.Runtime.CompilerServices;

namespace Delta.Engine.EditorLib.Scripting;

internal static class AccessorValuePathWriter
{
    public static unsafe void Write<TValue>(IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path, TValue value)
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
            Unsafe.AsRef<TValue>(address.ToPointer()) = value;
            return;
        }

        throw new InvalidOperationException($"Accessor type {type} does not match requested value type {typeof(TValue)}.");
    }
}
