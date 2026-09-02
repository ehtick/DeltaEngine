using Arch.Core;

namespace Delta.Engine.ECS;

internal sealed class HierarchyTreeRoots
{
    private readonly HierarchyTree _tree;

    public HierarchyTreeRoots(HierarchyTree tree)
    {
        _tree = tree;
    }

    public EntityReference[] Get()
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
}
