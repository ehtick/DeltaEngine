using Arch.Core;
using Arch.Core.Utils;
using Delta.Engine.ECS.Attributes;
using Delta.Engine.ECS.Components;
using Delta.Engine.Utilities;
using System;

namespace Delta.Engine.ECS;

public static class DirtyQueryExtensions
{
    [Imp(Inl)]
    public static void InlineDirtyQuery<T, T0, T1>(this World world, in QueryDescription description, ref T iForEach)
        where T : struct, IForEach<T0, T1>
    {
        ArgumentNullException.ThrowIfNull(world);
        world.AddDirty<T0>(description);
        world.AddDirty<T1>(description);
        world.InlineQuery<T, T0, T1>(description, ref iForEach);
    }

    [Imp(Inl)]
    private static void AddDirty<T>(this World world, in QueryDescription description)
    {
        if (AttributeCache.HasAttribute<DirtyAttribute, T>())
        {
            var desc = DirtyQueryDescriptionCache.GetNonDirty<T>(description);
            world.Add<DirtyFlag<T>>(desc);
        }
    }

    public static void MarkDirty<T>(this Entity entity)
    {
        if (entity.Has<T>() && !entity.Has<DirtyFlag<T>>() && AttributeCache.HasAttribute<DirtyAttribute, T>())
        {
            entity.Add<DirtyFlag<T>>();
        }
    }

    [Imp(Sync)]
    public static void MarkDirty(this Entity entity, Type component)
    {
        DirtyFlagCache.MarkDirty(entity, component);
    }
}
