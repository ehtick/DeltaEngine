using Delta.Engine.ECS.Attributes;
using System;

namespace Delta.Engine.ECS.Components;

[Component, Dirty]
public struct EntityName : IEquatable<EntityName>
{
    public string name;
    public EntityName(string name) => this.name = name;
    public EntityName() : this(string.Empty) { }

    public readonly bool Equals(EntityName other) => string.Equals(name, other.name, StringComparison.Ordinal);

    public override readonly bool Equals(object? obj) => obj is EntityName other && Equals(other);

    public override readonly int GetHashCode() => StringComparer.Ordinal.GetHashCode(name);

    public static bool operator ==(EntityName left, EntityName right) => left.Equals(right);

    public static bool operator !=(EntityName left, EntityName right) => !left.Equals(right);
}
