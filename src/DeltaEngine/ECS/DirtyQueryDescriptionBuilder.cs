using Arch.Core.Utils;
using System;

namespace Delta.Engine.ECS;

internal static class DirtyQueryDescriptionBuilder
{
    public static QueryDescription AddMissing(
        QueryDescription description,
        ComponentType component,
        ComponentType dirtyFlag)
    {
        description.All = AppendIfMissing(description.All, component);
        description.None = AppendIfMissing(description.None, dirtyFlag);
        return description;
    }

    private static ComponentType[] AppendIfMissing(ComponentType[] components, ComponentType component)
    {
        if (Array.IndexOf(components, component) >= 0)
        {
            return components;
        }

        var expanded = new ComponentType[components.Length + 1];
        components.CopyTo(expanded, 0);
        expanded[^1] = component;
        return expanded;
    }
}
