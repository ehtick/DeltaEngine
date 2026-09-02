using Arch.Core;
using Arch.Core.Extensions.Dangerous;
using Arch.Core.Utils;
using Delta.Engine.ECS;
using System;
using System.Runtime.CompilerServices;

namespace Delta.Engine.EditorLib.Scripting;

[Obsolete("The pointer-based legacy accessor helpers are migration-only; use neutral component accessors in DeltaEditor.", false)]
public static class AccessorContainerExtensions
{
    public static Type GetFieldType(this IAccessorsContainer container, Type type, ReadOnlySpan<string> path) =>
        AccessorPathTypeResolver.Resolve(container, type, path);

    public static unsafe TValue GetComponentFieldValue<TValue>(this IAccessorsContainer container, EntityReference entityReference, Type componentType, ReadOnlySpan<string> path)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(componentType);
        ref byte component = ref entityReference.GetComponentByteRef(componentType);
        return AccessorValuePathReader.Read<TValue>(container, componentType, new(Unsafe.AsPointer(ref component)), path);
    }

    public static unsafe void SetComponentFieldValue<TValue>(this IAccessorsContainer container, EntityReference entityReference, Type componentType, ReadOnlySpan<string> path, TValue value)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(componentType);
        ref byte component = ref entityReference.GetComponentByteRef(componentType);
        AccessorValuePathWriter.Write(container, componentType, new(Unsafe.AsPointer(ref component)), path, value);
    }

    public static unsafe TValue GetFieldValue<TValue>(this IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path) =>
        AccessorValuePathReader.Read<TValue>(container, type, address, path);

    public static unsafe void SetFieldValue<TValue>(this IAccessorsContainer container, Type type, nint address, ReadOnlySpan<string> path, TValue value) =>
        AccessorValuePathWriter.Write(container, type, address, path, value);
}
