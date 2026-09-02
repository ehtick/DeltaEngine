using Arch.Core;

namespace Delta.Engine.ECS;

internal sealed partial class HierarchyTree
{
    internal int GetChildIndex(EntityReference entityRef)
    {
        var target = GetNode(entityRef).Value;
        var index = 0;
        foreach (var node in GetParentNode(entityRef).Children)
        {
            if (node.Equals(target))
            {
                return index;
            }

            index++;
        }

        return -1;
    }
}
