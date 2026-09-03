using System;
using System.Collections.Generic;
using Delta.Maths;

namespace Delta.Engine.Integration;

public interface IEngineHierarchyReader
{
    uint Revision { get; }

    bool Contains(EngineEntityId entity);

    /// <summary>
    /// Copies the ordered roots into the caller buffer. Returns false when the
    /// buffer is too small; <paramref name="totalCount"/> is still the required
    /// capacity and the fitting prefix is copied.
    /// </summary>
    bool TryCopyRoots(Span<EngineEntityId> destination, out int totalCount);

    /// <summary>
    /// Copies ordered direct children into the caller buffer. Returns false
    /// when the parent is absent or the buffer is too small; in the latter case
    /// <paramref name="totalCount"/> is still the required capacity.
    /// </summary>
    bool TryCopyChildren(
        EngineEntityId parent,
        Span<EngineEntityId> destination,
        out int totalCount);
}

/// <summary>
/// Engine-owned hierarchy index for entity identity and deterministic sibling order.
/// ECS adapters feed structural changes into this index; the index does not own ECS storage.
/// </summary>
public sealed class EngineHierarchy : IEngineHierarchyReader, IDisposable
{
    private readonly Dictionary<EngineEntityId, Node> _nodes = [];
    private readonly List<EngineEntityId> _roots = [];
    private readonly List<EngineEntityId> _removeStack = [];
    private uint _revision;
    private bool _disposed;

    public uint Revision => _revision;

    public bool Contains(EngineEntityId entity)
    {
        ThrowIfDisposed();
        return _nodes.ContainsKey(entity);
    }

    public bool TryAdd(
        EngineEntityId entity,
        EngineEntityId? parent,
        out string? error)
    {
        ThrowIfDisposed();
        if (!entity.IsValid)
        {
            error = "The entity id is invalid.";
            return false;
        }

        if (_nodes.ContainsKey(entity))
        {
            error = "The entity is already present in the hierarchy.";
            return false;
        }

        if (!TryValidateParent(entity, parent, out error))
        {
            return false;
        }

        var node = new Node(entity, parent);
        _nodes.Add(entity, node);
        AddToParent(node);
        AdvanceRevision();
        error = null;
        return true;
    }

    public bool TrySetParent(
        EngineEntityId entity,
        EngineEntityId? parent,
        out string? error)
    {
        ThrowIfDisposed();
        if (!_nodes.TryGetValue(entity, out var node))
        {
            error = "The entity is not present in the hierarchy.";
            return false;
        }

        if (!TryValidateParent(entity, parent, out error))
        {
            return false;
        }

        if (node.Parent == parent)
        {
            error = null;
            return true;
        }

        RemoveFromParent(node);
        node.Parent = parent;
        AddToParent(node);
        AdvanceRevision();
        error = null;
        return true;
    }

    public bool TryRemoveSubtree(
        EngineEntityId entity,
        out int removedCount,
        out string? error)
    {
        ThrowIfDisposed();
        if (!_nodes.ContainsKey(entity))
        {
            removedCount = 0;
            error = "The entity is not present in the hierarchy.";
            return false;
        }

        _removeStack.Clear();
        _removeStack.Add(entity);
        removedCount = 0;
        while (_removeStack.Count != 0)
        {
            var lastIndex = _removeStack.Count - 1;
            var currentId = _removeStack[lastIndex];
            _removeStack.RemoveAt(lastIndex);
            if (!_nodes.Remove(currentId, out var current))
            {
                continue;
            }

            for (var childIndex = 0; childIndex < current.Children.Count; childIndex++)
            {
                _removeStack.Add(current.Children[childIndex]);
            }

            RemoveFromParent(current);
            removedCount++;
        }

        AdvanceRevision();
        error = null;
        return true;
    }

    public bool TryCopyRoots(Span<EngineEntityId> destination, out int totalCount)
    {
        ThrowIfDisposed();
        totalCount = _roots.Count;
        CopyPrefix(_roots, destination);
        return destination.Length >= totalCount;
    }

    public bool TryCopyChildren(
        EngineEntityId parent,
        Span<EngineEntityId> destination,
        out int totalCount)
    {
        ThrowIfDisposed();
        if (!_nodes.TryGetValue(parent, out var node))
        {
            totalCount = 0;
            return false;
        }

        totalCount = node.Children.Count;
        CopyPrefix(node.Children, destination);
        return destination.Length >= totalCount;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _nodes.Clear();
        _roots.Clear();
        _removeStack.Clear();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private bool TryValidateParent(
        EngineEntityId entity,
        EngineEntityId? parent,
        out string? error)
    {
        if (parent is not { } parentId)
        {
            error = null;
            return true;
        }

        if (!parentId.IsValid || !_nodes.ContainsKey(parentId))
        {
            error = "The parent is not present in the hierarchy.";
            return false;
        }

        var current = parentId;
        while (_nodes.TryGetValue(current, out var node) && node.Parent is { } ancestor)
        {
            if (ancestor == entity)
            {
                error = "The requested parent would create a hierarchy cycle.";
                return false;
            }

            current = ancestor;
        }

        if (parentId == entity)
        {
            error = "An entity cannot parent itself.";
            return false;
        }

        error = null;
        return true;
    }

    private void AddToParent(Node node)
    {
        if (node.Parent is { } parent)
        {
            _nodes[parent].Children.Add(node.Entity);
        }
        else
        {
            _roots.Add(node.Entity);
        }
    }

    private void RemoveFromParent(Node node)
    {
        if (node.Parent is { } parent)
        {
            if (_nodes.TryGetValue(parent, out var parentNode))
            {
                parentNode.Children.Remove(node.Entity);
            }
        }
        else
        {
            _roots.Remove(node.Entity);
        }
    }

    private static void CopyPrefix(List<EngineEntityId> source, Span<EngineEntityId> destination)
    {
        var count = maths.min(source.Count, destination.Length);
        for (var index = 0; index < count; index++)
        {
            destination[index] = source[index];
        }
    }

    private void AdvanceRevision()
    {
        _revision = _revision == uint.MaxValue ? 1u : _revision + 1u;
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private sealed class Node(EngineEntityId entity, EngineEntityId? parent)
    {
        public EngineEntityId Entity { get; } = entity;
        public EngineEntityId? Parent { get; set; } = parent;
        public List<EngineEntityId> Children { get; } = [];
    }
}
