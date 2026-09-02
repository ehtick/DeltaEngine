using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeSiblings
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeSiblings(HierarchyTree tree)
    {
        _tree = tree;
    }

    public EntityReference[] Get(EntityReference entityRef)
    {
        Debug.Assert(entityRef.Entity.Has<Order>());
        Debug.Assert(entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        var parentNode = _tree.GetParentNode(entityRef);
        var node = parentNode.Children.First;
        var count = parentNode.Children.Count;
        EntityReference[] siblings = new EntityReference[count];
        for (int i = 0; node is not null; i++, node = node.Next)
        {
            siblings[i] = node.Value.EntityReference;
        }

        return siblings;
    }
}
