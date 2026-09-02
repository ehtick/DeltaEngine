using Delta.ECS;
using Delta.Engine.ECS;
using Delta.Engine.ECS.Components;
using Delta.Maths;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class DeltaEcsTransformSystemTests
{
    [Fact]
    public void UpdatesRootAndChildWorldMatricesThroughDeltaEcs()
    {
        var layouts = new ComponentLayoutRegistry();
        var components = TransformComponentIds.Register(layouts);
        using var world = new World(layouts);

        Entity root = world.Create(stackalloc[] { components.WorldTransform, components.Transform });
        Entity child = world.Create(stackalloc[] { components.WorldTransform, components.Transform, components.Parent });
        var rootTransform = new Transform
        {
            position = new float3(2, 0, 0),
            rotation = quaternion.identity,
            scale = new float3(1),
        };
        var childTransform = new Transform
        {
            position = new float3(0, 3, 0),
            rotation = quaternion.identity,
            scale = new float3(1),
        };

        Assert.True(world.Set(root, components.Transform, rootTransform));
        Assert.True(world.Set(root, components.WorldTransform, new WorldTransform()));
        Assert.True(world.Set(child, components.Transform, childTransform));
        Assert.True(world.Set(child, components.Parent, new Parent(root)));
        Assert.True(world.Set(child, components.WorldTransform, new WorldTransform()));

        new TransformSystem(world, components).Update();

        Assert.Equal(rootTransform.LocalMatrix, world.Get<WorldTransform>(root, components.WorldTransform).Matrix);
        Assert.Equal(
            rootTransform.LocalMatrix * childTransform.LocalMatrix,
            world.Get<WorldTransform>(child, components.WorldTransform).Matrix);
    }
}
