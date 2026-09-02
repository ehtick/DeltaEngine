using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Attributes;
using Delta.Engine.ECS.Components;
using Delta.Engine.Utilities;
using System;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

internal static class DirtyFlagCache
{
    private static readonly Dictionary<Type, (Type Type, object Flag)> _flags = [];
    private static readonly Type DirtyFlagGeneric = typeof(DirtyFlag<>);

    public static void MarkDirty(Entity entity, Type component)
    {
        ArgumentNullException.ThrowIfNull(component);
        if (!entity.Has(component) || !AttributeCache.HasAttribute<DirtyAttribute>(component))
        {
            return;
        }

        if (!_flags.TryGetValue(component, out var entry))
        {
            var type = DirtyFlagGeneric.MakeGenericType(component);
            var flag = Activator.CreateInstance(type) ??
                throw new InvalidOperationException($"Unable to create dirty flag for {component.FullName}.");
            _flags[component] = entry = (type, flag);
        }

        if (!entity.Has(entry.Type))
        {
            entity.Add(entry.Flag);
        }
    }
}
