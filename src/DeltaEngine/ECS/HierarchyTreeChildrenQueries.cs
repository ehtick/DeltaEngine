using Arch.Core;
using Arch.Core.Extensions;
using System.Collections.Generic;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeChildrenQueries
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeChildrenQueries(HierarchyTree tree)
    {
        _tree = tree;
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

    private static void AppendChildren(List<EntityReference> entities, LinkedList<HierarchyTreeNode> children)
    {
        foreach (var item in children)
        {
            entities.Add(item.EntityReference);
            AppendChildren(entities, item.Children);
        }
    }
}
