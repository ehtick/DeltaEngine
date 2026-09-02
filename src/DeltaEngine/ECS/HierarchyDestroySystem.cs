using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using Delta.Engine.Runtime;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

internal readonly struct HierarchyDestroySystem(HierarchyTree hierarchy) : ISystem
{
    private static readonly QueryDescription _destroyDescription = new QueryDescription()
        .WithAll<DestroyFlag, HierarchySystem.HierarchyFlag, Order>();
    private static readonly List<Entity> _entitiesToDestroy = [];

    public readonly void Execute()
    {
        var world = IRuntimeContext.Current.SceneManager.CurrentScene._world;

        _entitiesToDestroy.Clear();
        world.GetEntities(_destroyDescription, _entitiesToDestroy);
        if (_entitiesToDestroy.Count == 0)
        {
            return;
        }

        _entitiesToDestroy.RemoveAll(static x => x.GetLastParent<DestroyFlag>(out var _));
        foreach (var entity in _entitiesToDestroy)
        {
            var entityRef = world.Reference(entity);
            var parentNode = hierarchy.GetParentNode(entityRef);
            var node = hierarchy.GetNode(entityRef);

            RemoveEntities(node.Value);
            parentNode.Children.Remove(node);
            hierarchy.CacheNode(node);
        }
    }

    private readonly void RemoveEntities(HierarchyTreeNode treeNode)
    {
        foreach (var item in treeNode.Children)
        {
            RemoveEntities(item);
        }

        treeNode.EntityReference.Entity.AddOrGet<DestroyFlag>();
        hierarchy.RemoveNode(treeNode);
    }
}
