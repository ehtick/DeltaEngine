using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeOrderQueries
{
    private readonly HierarchyTreeOrderIndex _orderIndex;
    private readonly HierarchyTreeRoots _roots;
    private readonly HierarchyTreeSiblings _siblings;

    public HierarchyTreeOrderQueries(HierarchyTree tree)
    {
        _orderIndex = new HierarchyTreeOrderIndex(tree);
        _roots = new HierarchyTreeRoots(tree);
        _siblings = new HierarchyTreeSiblings(tree);
    }

    public int GetOrderIndex(EntityReference entityRef) => _orderIndex.Get(entityRef);

    public EntityReference[] GetRootEntities() => _roots.Get();

    public EntityReference[] GetSiblings(EntityReference entityRef) => _siblings.Get(entityRef);
}
