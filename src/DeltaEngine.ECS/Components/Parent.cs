using Delta.ECS;

namespace Delta.Engine.ECS.Components;

/// <summary>Optional parent link for a DeltaECS transform hierarchy.</summary>
public readonly struct Parent : IEquatable<Parent>
{
    public Parent(Entity entity) => Entity = entity;

    public Entity Entity { get; }

    public bool Equals(Parent other) => Entity == other.Entity;

    public override bool Equals(object? obj) => obj is Parent other && Equals(other);

    public override int GetHashCode() => Entity.GetHashCode();

    public static bool operator ==(Parent left, Parent right) => left.Equals(right);

    public static bool operator !=(Parent left, Parent right) => !left.Equals(right);
}
