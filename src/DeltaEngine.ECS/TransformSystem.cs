using Delta.ECS;
using Delta.Engine.ECS.Components;
using Delta.Maths;

namespace Delta.Engine.ECS;

/// <summary>
/// Updates derived world matrices from local transforms and optional parents.
/// The system owns no scheduler; callers invoke <see cref="Update"/> explicitly.
/// </summary>
public sealed class TransformSystem
{
    private const int MaxHierarchyDepth = 1024;

    private readonly World _world;
    private readonly TransformComponentIds _components;
    private readonly Query _rootQuery;
    private readonly Query _childQuery;

    public TransformSystem(World world, TransformComponentIds components)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
        _components = components;
        EnsureComponentType<Transform>(world, components.Transform, nameof(components.Transform));
        EnsureComponentType<Parent>(world, components.Parent, nameof(components.Parent));
        EnsureComponentType<WorldTransform>(world, components.WorldTransform, nameof(components.WorldTransform));

        var rootSpec = new QuerySpec(
            stackalloc[] { components.WorldTransform, components.Transform },
            ReadOnlySpan<ComponentId>.Empty,
            stackalloc[] { components.Parent });
        var childSpec = QuerySpec.WhereAll(components.WorldTransform, components.Transform, components.Parent);
        _rootQuery = world.CreateQuery(in rootSpec);
        _childQuery = world.CreateQuery(in childSpec);
    }

    public void Update()
    {
        var roots = new RootTransformFunctor();
        _world.ForEach(in _rootQuery, ref roots);

        var children = new ChildTransformFunctor(_world, _components.Transform, _components.Parent);
        _world.ForEach(in _childQuery, ref children);
    }

    private static void EnsureComponentType<T>(World world, ComponentId component, string name)
    {
        if (!world.Layouts.TryGet(component, out var layout) || layout.RuntimeType != typeof(T))
        {
            throw new ArgumentException(
                $"Transform component registration {name} must contain {typeof(T)}.",
                nameof(component));
        }
    }

    internal struct RootTransformFunctor : IForEach
    {
        public void Invoke(ref WorldTransform worldTransform, in Transform transform)
            => worldTransform.Matrix = transform.LocalMatrix;
    }

    internal struct ChildTransformFunctor : IForEach
    {
        private readonly World _world;
        private readonly ComponentId _transform;
        private readonly ComponentId _parent;

        public ChildTransformFunctor(World world, ComponentId transform, ComponentId parent)
        {
            _world = world;
            _transform = transform;
            _parent = parent;
        }

        public void Invoke(
            ref WorldTransform worldTransform,
            in Transform transform,
            in Parent parent)
            => worldTransform.Matrix = ResolveWorld(parent.Entity, 0) * transform.LocalMatrix;

        private float4x4 ResolveWorld(Entity entity, int depth)
        {
            if (depth >= MaxHierarchyDepth)
            {
                throw new InvalidOperationException("Transform hierarchy exceeds the maximum depth or contains a cycle.");
            }

            if (!_world.IsAlive(entity))
            {
                return float4x4.identity;
            }

            if (!_world.TryGet(entity, _transform, out Transform local))
            {
                if (_world.TryGet(entity, _parent, out Parent missingTransformParent))
                {
                    return ResolveWorld(missingTransformParent.Entity, depth + 1);
                }

                return float4x4.identity;
            }

            if (!_world.TryGet(entity, _parent, out Parent parent)
                || !parent.Entity.IsAlive
                || parent.Entity == entity
                || !_world.IsAlive(parent.Entity))
            {
                return local.LocalMatrix;
            }

            return ResolveWorld(parent.Entity, depth + 1) * local.LocalMatrix;
        }
    }
}
