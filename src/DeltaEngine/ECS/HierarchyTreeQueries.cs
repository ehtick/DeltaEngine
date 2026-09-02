using Arch.Core;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeQueries
{
    private readonly HierarchyTreeOrderQueries _orderQueries;
    private readonly HierarchyTreeChildrenQueries _childrenQueries;

    public HierarchyTreeQueries(HierarchyTree tree)
    {
        _orderQueries = new HierarchyTreeOrderQueries(tree);
        _childrenQueries = new HierarchyTreeChildrenQueries(tree);
    }

    public int GetOrderIndex(EntityReference entityRef) => _orderQueries.GetOrderIndex(entityRef);

    public EntityReference[] GetRootEntities() => _orderQueries.GetRootEntities();

    public void GetChildren(EntityReference entityRef, List<EntityReference> children)
        => _childrenQueries.GetChildren(entityRef, children);

    public List<EntityReference> GetFirstChildren(EntityReference entityRef)
        => _childrenQueries.GetFirstChildren(entityRef);

    public void GetFirstChildren(EntityReference entityRef, List<EntityReference> children)
        => _childrenQueries.GetFirstChildren(entityRef, children);

    public int GetFirstChildrenCount(EntityReference entityRef)
        => _childrenQueries.GetFirstChildrenCount(entityRef);

    public List<EntityReference> GetChildren(EntityReference entityRef)
        => _childrenQueries.GetChildren(entityRef);

    public EntityReference[] GetSiblings(EntityReference entityRef) => _orderQueries.GetSiblings(entityRef);
}
