using Arch.Core;
using System;
using System.Collections.Generic;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeNode : IEquatable<HierarchyTreeNode>
{
    public EntityReference EntityReference;
    public readonly LinkedList<HierarchyTreeNode> Children = [];

    public HierarchyTreeNode(EntityReference entityReference)
    {
        EntityReference = entityReference;
    }

    public HierarchyTreeNode() : this(EntityReference.Null) { }

    public override bool Equals(object? obj) => obj is HierarchyTreeNode node && Equals(node);

    public bool Equals(HierarchyTreeNode? other)
        => other is not null && other.EntityReference == EntityReference;

    public override int GetHashCode() => EntityReference.GetHashCode();

    public static bool operator ==(HierarchyTreeNode left, HierarchyTreeNode right) => left.Equals(right);

    public static bool operator !=(HierarchyTreeNode left, HierarchyTreeNode right) => !left.Equals(right);
}
