using Arch.Core;
using Arch.Core.Extensions;
using Delta.Engine.ECS.Components;
using System.Collections.Generic;
using System.Diagnostics;

namespace Delta.Engine.ECS;

internal sealed partial class HierarchyTree
{
    private readonly Dictionary<EntityReference, LinkedListNode<HierarchyTreeNode>> _entityToNode = [];
    private readonly HierarchyTreeNode _root = new();
    private readonly Stack<LinkedListNode<HierarchyTreeNode>> _cachedNodes = [];
    private readonly Stack<HierarchyTreeNode> _cachedTreeNodes = [];

    public int RootEntitiesCount => _root.Children.Count;

    public int EntitiesCount => _entityToNode.Count;

    public ISystem CreateDestroySystem() => new HierarchyDestroySystem(this);

    public void UpdateOrders() => UpdateOrders(_root.Children);

    public void AddRootEntity(EntityReference entityRef)
    {
        Debug.Assert(!entityRef.Entity.Has<ChildOf>());
        Debug.Assert(!entityRef.Entity.Has<Order>());
        Debug.Assert(!entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        var order = _root.Children.Count;
        CreateMarkerAndOrder(entityRef, order);
        var node = GetOrCreateNode();
        _entityToNode[entityRef] = node;
        node.Value.EntityReference = entityRef;
        _root.Children.AddLast(node);
    }

    internal LinkedList<HierarchyTreeNode> RootChildren => _root.Children;

    internal LinkedListNode<HierarchyTreeNode> GetNode(EntityReference entityRef)
        => _entityToNode[entityRef];

    internal HierarchyTreeNode GetParentNode(EntityReference entityRef)
    {
        var entity = entityRef.Entity;
        Debug.Assert(entity.Has<HierarchySystem.HierarchyFlag>());

        var parent = _root;
        if (entity.TryGet<ChildOf>(out var childOf))
        {
            Debug.Assert(childOf.parent.Entity.Has<HierarchySystem.HierarchyFlag>());
            parent = _entityToNode[childOf.parent].Value;
        }

        return parent;
    }

    internal void RemoveNode(HierarchyTreeNode treeNode)
    {
        var node = treeNode.Children.First;
        while (node is not null)
        {
            _cachedNodes.Push(node);
            node = node.Next;
        }

        treeNode.Children.Clear();
        treeNode.EntityReference = EntityReference.Null;
        _cachedTreeNodes.Push(treeNode);
    }

    internal void CacheNode(LinkedListNode<HierarchyTreeNode> node) => _cachedNodes.Push(node);

    private static void UpdateOrders(LinkedList<HierarchyTreeNode> children)
    {
        var node = children.First;
        for (int i = 0; node is not null; i++, node = node.Next)
        {
            node.Value.EntityReference.Entity.Get<Order>().order = i;
            UpdateOrders(node.Value.Children);
        }
    }

    private LinkedListNode<HierarchyTreeNode> GetOrCreateNode()
    {
        if (!_cachedNodes.TryPop(out var node))
        {
            node = new LinkedListNode<HierarchyTreeNode>(new HierarchyTreeNode());
        }
        else if (_cachedTreeNodes.TryPop(out var cachedTreeNode))
        {
            node.Value = cachedTreeNode;
        }

        return node;
    }

    private static void CreateMarkerAndOrder(EntityReference entityRef, int order)
    {
        Debug.Assert(!entityRef.Entity.Has<Order>());
        Debug.Assert(!entityRef.Entity.Has<HierarchySystem.HierarchyFlag>());

        entityRef.Entity.Add(new HierarchySystem.HierarchyFlag(), new Order(order));
    }
}
