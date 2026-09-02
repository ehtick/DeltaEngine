using System;

namespace Delta.Engine.EditorLib.Scripting;

internal static class AccessorPathTypeResolver
{
    public static Type Resolve(IAccessorsContainer container, Type type, ReadOnlySpan<string> path)
    {
        ArgumentNullException.ThrowIfNull(container);
        ArgumentNullException.ThrowIfNull(type);
        foreach (var fieldName in path)
        {
            type = container.AllAccessors[type].GetFieldType(fieldName);
        }

        return type;
    }
}
