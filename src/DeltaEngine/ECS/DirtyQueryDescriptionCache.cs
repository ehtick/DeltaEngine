using Arch.Core;
using Arch.Core.Utils;
using Delta.Engine.ECS.Components;
using System;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

internal static class DirtyQueryDescriptionCache
{
    private static readonly Dictionary<QueryDescription, Dictionary<ComponentType, QueryDescription>> _lookup = [];

    public static QueryDescription GetNonDirty<T>(QueryDescription description)
    {
        var component = Component<T>.ComponentType;
        var dirtyFlag = Component<DirtyFlag<T>>.ComponentType;
        if (!_lookup.TryGetValue(description, out var descriptions))
        {
            _lookup[description] = descriptions = [];
        }

        if (!descriptions.TryGetValue(component, out var result))
        {
            result = DirtyQueryDescriptionBuilder.AddMissing(description, component, dirtyFlag);
            descriptions[component] = result;
        }

        return result;
    }

}
