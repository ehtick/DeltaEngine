using Delta.ECS;
using Delta.ECS.Integration;

namespace Delta.Engine.Integration;

/// <summary>Neutral parent component consumed by the DeltaECS hierarchy adapter.</summary>
public readonly record struct EngineHierarchyParent(EngineEntityId Parent)
{
    public bool IsRoot => !Parent.IsValid;
}

/// <summary>
/// Bridges explicit DeltaECS entity lifetime notifications into <see cref="EngineHierarchy"/>.
/// The caller owns entity resolution and invokes the adapter at a controlled frame boundary.
/// This is a cold structural/tooling boundary, not a per-frame render or update path.
/// </summary>
public sealed class DeltaEcsHierarchyAdapter : IDisposable
{
    private readonly IEcsWorld _world;
    private readonly ComponentId _parentComponent;
    private readonly EngineHierarchy _hierarchy = new();
    private readonly Dictionary<EngineEntityId, Entity> _entities = [];
    private readonly List<EngineEntityId> _staleEntities = [];
    private bool _disposed;

    public DeltaEcsHierarchyAdapter(IEcsWorld world, ComponentId parentComponent)
    {
        ArgumentNullException.ThrowIfNull(world);
        if (!parentComponent.IsValid)
        {
            throw new ArgumentException("The parent component id must be valid.", nameof(parentComponent));
        }

        _world = world;
        _parentComponent = parentComponent;
    }

    public IEngineHierarchyReader Reader => _hierarchy;

    public bool TryAttach(
        EngineEntityId entity,
        Entity ecsEntity,
        out string? error)
    {
        ThrowIfDisposed();
        if (!entity.IsValid)
        {
            error = "The engine entity id is invalid.";
            return false;
        }

        if (_entities.ContainsKey(entity))
        {
            error = "The entity is already attached to the hierarchy adapter.";
            return false;
        }

        if (!_world.IsAlive(ecsEntity))
        {
            error = "The ECS entity is not alive.";
            return false;
        }

        if (!TryReadParent(ecsEntity, out var parent, out error))
        {
            return false;
        }

        if (!_hierarchy.TryAdd(entity, parent, out error))
        {
            return false;
        }

        _entities.Add(entity, ecsEntity);
        return true;
    }

    public bool TrySync(EngineEntityId entity, out string? error)
    {
        ThrowIfDisposed();
        if (!_entities.TryGetValue(entity, out var ecsEntity))
        {
            error = "The entity is not attached to the hierarchy adapter.";
            return false;
        }

        if (!_world.IsAlive(ecsEntity))
        {
            return TryDetach(entity, out _, out error);
        }

        if (!TryReadParent(ecsEntity, out var parent, out error))
        {
            return false;
        }

        return _hierarchy.TrySetParent(entity, parent, out error);
    }

    public bool TryDetach(
        EngineEntityId entity,
        out int removedCount,
        out string? error)
    {
        ThrowIfDisposed();
        if (!_entities.Remove(entity))
        {
            removedCount = 0;
            error = "The entity is not attached to the hierarchy adapter.";
            return false;
        }

        if (!_hierarchy.TryRemoveSubtree(entity, out removedCount, out error))
        {
            return false;
        }

        _staleEntities.Clear();
        foreach (var attached in _entities.Keys)
        {
            if (!_hierarchy.Contains(attached))
            {
                _staleEntities.Add(attached);
            }
        }

        for (var index = 0; index < _staleEntities.Count; index++)
        {
            _entities.Remove(_staleEntities[index]);
        }

        error = null;
        return true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _entities.Clear();
        _staleEntities.Clear();
        _hierarchy.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private bool TryReadParent(
        Entity ecsEntity,
        out EngineEntityId? parent,
        out string? error)
    {
        if (!_world.TryRead(ecsEntity, _parentComponent, out var snapshot, out var readError))
        {
            if (readError.Code == EcsReadErrorCode.ComponentMissing)
            {
                parent = null;
                error = null;
                return true;
            }

            parent = null;
            error = $"Unable to read hierarchy parent: {readError.Code}.";
            return false;
        }

        if (snapshot.Value is not EngineHierarchyParent parentValue)
        {
            parent = null;
            error = "The hierarchy parent component has an unexpected value type.";
            return false;
        }

        parent = parentValue.IsRoot ? null : parentValue.Parent;
        error = null;
        return true;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}
