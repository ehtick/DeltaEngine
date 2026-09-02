using Arch.Core;

namespace Delta.Engine.ECS;

internal static class HierarchyTreeOrderSearch
{
    /// <summary>Returns the entity position in its current linked-list sibling order.</summary>
    /// <remarks>The migration-only hierarchy keeps this order separately from the component value.</remarks>
    public static int Find(HierarchyTree tree, EntityReference entityRef) => tree.GetChildIndex(entityRef);
}
