using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using System.Collections.Generic;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeQueries
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeQueries(HierarchyTree tree)
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

    public void GetChildren(EntityReference entityRef, List<EntityReference> children)
    {
        Debug.Assert(entityRef.Entity.Has<Order>());
        Debug.Assert(entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        var node = _tree.GetNode(entityRef);
        AppendChildren(children, node.Value.Children);
    }

    public List<EntityReference> GetFirstChildren(EntityReference entityRef)
    {
        List<EntityReference> children = [];
        GetFirstChildren(entityRef, children);
        return children;
    }

    public void GetFirstChildren(EntityReference entityRef, List<EntityReference> children)
    {
        Debug.Assert(entityRef.Entity.Has<Order>());
        Debug.Assert(entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        var node = _tree.GetNode(entityRef);
        foreach (var item in node.Value.Children)
        {
            children.Add(item.EntityReference);
        }
    }

    public int GetFirstChildrenCount(EntityReference entityRef)
    {
        Debug.Assert(entityRef.Entity.Has<Order>());
        Debug.Assert(entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        return _tree.GetNode(entityRef).Value.Children.Count;
    }

    public List<EntityReference> GetChildren(EntityReference entityRef)
    {
        List<EntityReference> children = [];
        GetChildren(entityRef, children);
        return children;
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

    private static void AppendChildren(List<EntityReference> entities, LinkedList<HierarchyTreeNode> children)
    {
        foreach (var item in children)
        {
            entities.Add(item.EntityReference);
            AppendChildren(entities, item.Children);
        }
    }
}
