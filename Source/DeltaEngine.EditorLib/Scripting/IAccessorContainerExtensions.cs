using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Delta.Engine.ECS;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Delta.Engine.EditorLib.Scripting;

public static class AccessorContainerExtensions
{
    public static Type GetFieldType(this IAccessorsContainer container, Type type, ReadOnlySpan<string> path)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(type);
        foreach (var fieldName in path)
        {
            type = container.AllAccessors[type].GetFieldType(fieldName);
        }

        return type;
    }

    public static unsafe TValue GetComponentFieldValue<TValue>(this IAccessorsContainer container, EntityReference entityReference, Type componentType, ReadOnlySpan<string> path)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(componentType);
        ref byte cmpRef = ref entityReference.GetComponentByteRef(componentType);
        return container.GetFieldValue<TValue>(componentType, new(Unsafe.AsPointer(ref cmpRef)), path);
    }

    public static unsafe void SetComponentFieldValue<TValue>(this IAccessorsContainer container, EntityReference entityReference, Type componentType, ReadOnlySpan<string> path, TValue value)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(componentType);
        ref byte cmpRef = ref entityReference.GetComponentByteRef(componentType);
        container.SetFieldValue(componentType, new(Unsafe.AsPointer(ref cmpRef)), path, value);
    }

    public static unsafe TValue GetFieldValue<TValue>(this IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path)
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

    public static unsafe void SetFieldValue<TValue>(this IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path, TValue value)
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
