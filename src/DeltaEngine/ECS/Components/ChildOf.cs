using Arch.Core;
using System;

namespace Delta.Engine.ECS.Components;

/// <summary>
/// Stores information about parent of entity.
/// Can be used for world TRS calculations for rendering
/// or other child/parent dependencies
/// </summary>
[Obsolete("Arch-backed ChildOf is migration-only; use EngineHierarchy with EngineEntityId.", false)]
public readonly struct ChildOf : IEquatable<ChildOf>
{
    /// <summary>
    /// Parent of entity, containing <see cref="ChildOf"/> component
    /// </summary>
    public readonly EntityReference parent;

    public ChildOf(EntityReference parent)
    {
        this.parent = parent;
    }

    public bool Equals(ChildOf other) => parent.Equals(other.parent);

    public override bool Equals(object? obj) => obj is ChildOf other && Equals(other);

    public override int GetHashCode() => parent.GetHashCode();

    public static bool operator ==(ChildOf left, ChildOf right) => left.Equals(right);

    public static bool operator !=(ChildOf left, ChildOf right) => !left.Equals(right);
}
