using Arch.Core;
using System;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

[Obsolete("Arch-backed hierarchy is migration-only; use Delta.Engine.Integration.EngineHierarchy.", false)]
internal sealed class HierarchySystem
{
    private readonly HierarchyTree _tree = new();
    private readonly HierarchyTreeQueries _queries;

    public HierarchySystem()
    {
        _queries = new HierarchyTreeQueries(_tree);
    }

    public ISystem MarkDestroySystem() => _tree.CreateDestroySystem();

    public int GetOrderIndex(EntityReference entityRef) => _queries.GetOrderIndex(entityRef);

    public int RootEntitiesCount => _tree.RootEntitiesCount;

    public int EntitiesCount => _tree.EntitiesCount;

    public void UpdateOrders() => _tree.UpdateOrders();

    public EntityReference[] GetRootEntities() => _queries.GetRootEntities();

    public void GetChildren(EntityReference entityRef, List<EntityReference> children)
        => _queries.GetChildren(entityRef, children);

    public List<EntityReference> GetFirstChildren(EntityReference entityRef)
        => _queries.GetFirstChildren(entityRef);

    public void GetFirstChildren(EntityReference entityRef, List<EntityReference> children)
        => _queries.GetFirstChildren(entityRef, children);

    public int GetFirstChildrenCount(EntityReference entityRef)
        => _queries.GetFirstChildrenCount(entityRef);

    public List<EntityReference> GetChildren(EntityReference entityRef)
        => _queries.GetChildren(entityRef);

    public EntityReference[] GetSiblings(EntityReference entityRef)
        => _queries.GetSiblings(entityRef);

    [Imp(Sync)]
    public void AddRootEntity(EntityReference entityRef) => _tree.AddRootEntity(entityRef);

    internal struct HierarchyFlag { }
}
