using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeOrderIndex
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeOrderIndex(HierarchyTree tree)
    {
        _tree = tree;
    }

    public int Get(EntityReference entityRef)
    {
        Debug.Assert(entityRef.Entity.Has<Order>());
        Debug.Assert(entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());
        return HierarchyTreeOrderSearch.Find(_tree, entityRef);
    }
}
