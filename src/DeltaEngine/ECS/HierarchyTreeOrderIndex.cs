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

        var parentNode = _tree.GetParentNode(entityRef);
        var search = _tree.GetNode(entityRef);
        var node = parentNode.Children.First;
        for (int i = 0; node is not null; i++, node = node.Next)
        {
            if (node.Value.Equals(search.Value))
            {
                return i;
            }
        }

        Debug.Assert(false);
        return -1;
    }
}
