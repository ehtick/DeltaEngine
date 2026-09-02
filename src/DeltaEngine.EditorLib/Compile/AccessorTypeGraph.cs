using Delta.Engine.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

internal static class AccessorTypeGraph
{
    public static HashSet<Type> Collect(IEnumerable<Type> componentTypes)
    {
        HashSet<Type> visited = [];
        foreach (var type in componentTypes)
        {
            if (type.IsPublic)
            {
                Add(type, visited);
            }
        }

        return visited;
    }

    public static void Add(Type type, HashSet<Type> visited)
    {
        if (!visited.Add(type))
        {
            return;
        }

        foreach (var field in SelectFields(type.GetFields()))
        {
            if (!field.FieldType.IsPrimitive && field.FieldType != typeof(string))
            {
                Add(field.FieldType, visited);
            }
        }
    }

    public static IEnumerable<FieldInfo> SelectFields(IEnumerable<FieldInfo> fields) =>
        fields.Where(static field =>
            field.FieldType.IsPublic &&
            !field.IsStatic &&
            (field.IsPublic || field.IsDefined(typeof(EditableAttribute), false)));
}
