using Delta.ECS;
using Delta.Engine.ECS;
using Delta.Engine.ECS.Components;

var layouts = new ComponentLayoutRegistry();
var components = TransformComponentIds.Register(layouts);
using var world = new World(layouts);

Entity root = world.Create(stackalloc[] { components.WorldTransform, components.Transform });
Entity child = world.Create(stackalloc[] { components.WorldTransform, components.Transform, components.Parent });

var rootTransform = new Transform
{
    position = new Delta.float3(2, 0, 0),
    rotation = Delta.quaternion.identity,
    scale = new Delta.float3(1),
};
var childTransform = new Transform
{
    position = new Delta.float3(0, 3, 0),
    rotation = Delta.quaternion.identity,
    scale = new Delta.float3(1),
};

_ = world.Set(root, components.Transform, rootTransform);
_ = world.Set(root, components.WorldTransform, new WorldTransform());
_ = world.Set(child, components.Transform, childTransform);
_ = world.Set(child, components.Parent, new Parent(root));
_ = world.Set(child, components.WorldTransform, new WorldTransform());

var transformSystem = new TransformSystem(world, components);
transformSystem.Update();

WorldTransform childWorld = world.Get<WorldTransform>(child, components.WorldTransform);
Console.WriteLine($"Updated {child}: {childWorld.Matrix}");
