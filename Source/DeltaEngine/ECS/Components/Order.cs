using System;

namespace DeltaEngine.ECS.Components;

/// <summary>
/// Stores information about order of entity in hierarchy.
/// Root entity does not have <see cref="ChildOf"/> component,
/// but still contains <see cref="Order"/> component with <see cref="order"/>
/// Each <see cref="order"/> value is unique for each <see cref="ChildOf.parent"/> group
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1051:Do not declare visible instance fields",
    Justification = "ECS component fields are public by design for generated ref access.")]
public struct Order : IEquatable<Order>
{
    public int order;

    public Order(int order)
    {
        this.order = order;
    }

    public readonly bool Equals(Order other) => order == other.order;

    public override readonly bool Equals(object? obj) => obj is Order other && Equals(other);

    public override readonly int GetHashCode() => order;

    public static bool operator ==(Order left, Order right) => left.Equals(right);

    public static bool operator !=(Order left, Order right) => !left.Equals(right);
}
