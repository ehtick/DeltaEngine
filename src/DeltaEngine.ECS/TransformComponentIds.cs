using Delta.ECS;
using Delta.Engine.ECS.Components;

namespace Delta.Engine.ECS;

/// <summary>Stable DeltaECS registrations used by the transform feature.</summary>
public readonly record struct TransformComponentIds(
    ComponentId Transform,
    ComponentId Parent,
    ComponentId WorldTransform)
{
    private const ulong TransformSchema = 0x4445_4C54_4145_4353UL;

    public static TransformComponentIds Register(ComponentLayoutRegistry layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);

        return new TransformComponentIds(
            layouts.Register<Transform>(new SchemaId(TransformSchema)),
            layouts.Register<Parent>(new SchemaId(TransformSchema + 1)),
            layouts.Register<WorldTransform>(new SchemaId(TransformSchema + 2)));
    }
}
