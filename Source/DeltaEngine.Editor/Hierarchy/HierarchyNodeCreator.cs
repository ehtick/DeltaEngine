using Arch.Core;
using DeltaEngine.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DeltaEngine.Editor.Hierarchy
{
    public sealed class EntityReferenceEventArgs(EntityReference entity) : EventArgs
    {
        public EntityReference Entity { get; } = entity;
    }

    public sealed class HierarchyNodeCreator
    {
        private readonly Stack<HierarchyNodeControl> _nodes = [];
        private readonly HashSet<EntityReference> _expandedNodes = [];

        private readonly List<EntityReference> _childrenListCached = [];

        public event EventHandler<EntityReferenceEventArgs>? OnEntitySelectRequest;
        public event EventHandler<EntityReferenceEventArgs>? OnEntityRemoveRequest;

        public void CallRemove(EntityReference entityRef) => OnEntityRemoveRequest?.Invoke(this, new EntityReferenceEventArgs(entityRef));
        public void CallSelect(EntityReference entityRef) => OnEntitySelectRequest?.Invoke(this, new EntityReferenceEventArgs(entityRef));


        public bool IsCollapsed(EntityReference entityRef)
        {
            return !_expandedNodes.Contains(entityRef);
        }

        public void SetCollapsed(EntityReference entityRef, bool collapsed)
        {
            if (collapsed)
            {
                _expandedNodes.Remove(entityRef);
            }
            else
            {
                _expandedNodes.Add(entityRef);
            }
        }

        public ReadOnlySpan<EntityReference> GetChildren(EntityReference entityRef)
        {
            IRuntimeContext.Current.SceneManager.CurrentScene.GetFirstChildren(entityRef, _childrenListCached);
            return CollectionsMarshal.AsSpan(_childrenListCached);
        }

        public int GetChildrenCount(EntityReference entityRef)
        {
            return IRuntimeContext.Current.SceneManager.CurrentScene.GetFirstChildrenCount(entityRef);
        }

        public HierarchyNodeControl GetOrCreateNode()
        {
            return _nodes.TryPop(out var node)
                ? node
                : new HierarchyNodeControl(this);
        }

        public void ReturnNode(HierarchyNodeControl node)
        {
            _nodes.Push(node);
        }
    }
}
