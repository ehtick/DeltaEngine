using Arch.Core;
using Arch.Core.Extensions;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeOrderQueries
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeOrderQueries(HierarchyTree tree)
    {
        _tree = tree;
    }

    public int GetOrderIndex(EntityReference entityRef)
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

    public EntityReference[] GetRootEntities()
    {
        var children = _tree.RootChildren;
        var count = children.Count;
        var node = children.First;
        if (node is null)
        {
            return [];
        }

        EntityReference[] references = new EntityReference[count];
        for (int i = 0; node is not null; i++, node = node.Next)
        {
            references[i] = node.Value.EntityReference;
        }

        return references;
    }

    public EntityReference[] GetSiblings(EntityReference entityRef)
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
