using System;
using Delta.Engine.Integration;
using Delta.ECS;
using Delta.ECS.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EngineHierarchyContractTests
{
    [Fact]
    public void MaintainsDeterministicOrderAndCopiesOnlyIntoCallerStorage()
    {
        using var hierarchy = new EngineHierarchy();
        var root = new EngineEntityId(1);
        var first = new EngineEntityId(2);
        var second = new EngineEntityId(3);

        Assert.True(hierarchy.TryAdd(root, null, out _));
        Assert.True(hierarchy.TryAdd(first, root, out _));
        Assert.True(hierarchy.TryAdd(second, root, out _));

        Span<EngineEntityId> roots = stackalloc EngineEntityId[1];
        Assert.True(hierarchy.TryCopyRoots(roots, out var rootCount));
        Assert.Equal(1, rootCount);
        Assert.Equal(root, roots[0]);

        Span<EngineEntityId> children = stackalloc EngineEntityId[1];
        Assert.False(hierarchy.TryCopyChildren(root, children, out var childCount));
        Assert.Equal(2, childCount);
        Assert.Equal(first, children[0]);

        Span<EngineEntityId> allChildren = stackalloc EngineEntityId[2];
        Assert.True(hierarchy.TryCopyChildren(root, allChildren, out childCount));
        Assert.Equal(2, childCount);
        Assert.Equal(first, allChildren[0]);
        Assert.Equal(second, allChildren[1]);
    }

    [Fact]
    public void RejectsCyclesAndRemovesSubtreesWithoutInvalidatingCallerViews()
    {
        using var hierarchy = new EngineHierarchy();
        var root = new EngineEntityId(1);
        var child = new EngineEntityId(2);
        var grandchild = new EngineEntityId(3);

        Assert.True(hierarchy.TryAdd(root, null, out _));
        Assert.True(hierarchy.TryAdd(child, root, out _));
        Assert.True(hierarchy.TryAdd(grandchild, child, out _));
        Assert.False(hierarchy.TrySetParent(root, grandchild, out var cycleError));
        Assert.Contains("cycle", cycleError, StringComparison.OrdinalIgnoreCase);

        Assert.True(hierarchy.TryRemoveSubtree(child, out var removed, out _));
        Assert.Equal(2, removed);
        Assert.False(hierarchy.Contains(child));
        Assert.False(hierarchy.Contains(grandchild));

        Span<EngineEntityId> roots = stackalloc EngineEntityId[1];
        Assert.True(hierarchy.TryCopyRoots(roots, out var rootCount));
        Assert.Equal(1, rootCount);
        Assert.Equal(root, roots[0]);
    }

    [Fact]
    public void ReparentingPreservesNodeIdentityAndRevisionChangesOnlyOnMutation()
    {
        using var hierarchy = new EngineHierarchy();
        var root = new EngineEntityId(1);
        var otherRoot = new EngineEntityId(2);
        var child = new EngineEntityId(3);

        Assert.True(hierarchy.TryAdd(root, null, out _));
        Assert.True(hierarchy.TryAdd(otherRoot, null, out _));
        Assert.True(hierarchy.TryAdd(child, root, out _));
        var revision = hierarchy.Revision;

        Assert.True(hierarchy.TrySetParent(child, otherRoot, out _));
        Assert.True(hierarchy.Revision > revision);
        Assert.True(hierarchy.Contains(child));
        var unchangedRevision = hierarchy.Revision;
        Assert.True(hierarchy.TrySetParent(child, otherRoot, out _));
        Assert.Equal(unchangedRevision, hierarchy.Revision);
    }

    [Fact]
    public void DeltaEcsAdapterReadsParentAndRemovesDetachedSubtree()
    {
        using var world = new Delta.ECS.World();
        var parentComponent = world.Layouts.Register<EngineHierarchyParent>(SchemaId.FromUInt64(100));
        var ecsWorld = (IEcsWorld)world;
        ecsWorld.Initialize();

        var rootEntity = ecsWorld.Create(ReadOnlySpan<ComponentId>.Empty);
        var childEntity = ecsWorld.Create(ReadOnlySpan<ComponentId>.Empty);
        Span<ComponentId> parentComponents = stackalloc ComponentId[1];
        parentComponents[0] = parentComponent;
        Assert.True(ecsWorld.Add(childEntity, parentComponents));

        using var adapter = new DeltaEcsHierarchyAdapter(ecsWorld, parentComponent);
        var root = new EngineEntityId(10);
        var child = new EngineEntityId(11);
        Assert.True(adapter.TryAttach(root, rootEntity, out _));
        Assert.True(adapter.TryAttach(child, childEntity, out _));

        Assert.True(ecsWorld.TryRead(childEntity, parentComponent, out var snapshot, out _));
        Assert.True(ecsWorld.TryWrite(
            childEntity,
            parentComponent,
            new EngineHierarchyParent(root),
            snapshot.Stamp,
            out _,
            out _));
        Assert.True(adapter.TrySync(child, out _));

        Span<EngineEntityId> children = stackalloc EngineEntityId[1];
        Assert.True(adapter.Reader.TryCopyChildren(root, children, out var childCount));
        Assert.Equal(1, childCount);
        Assert.Equal(child, children[0]);

        Assert.True(adapter.TryDetach(root, out var removedCount, out _));
        Assert.Equal(2, removedCount);
        Assert.False(adapter.Reader.Contains(root));
        Assert.False(adapter.Reader.Contains(child));
        ecsWorld.Shutdown();
    }
}
