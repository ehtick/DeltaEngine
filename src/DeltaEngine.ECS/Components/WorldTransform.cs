using Delta;

namespace Delta.Engine.ECS.Components;

/// <summary>Derived world matrix written by <see cref="TransformSystem"/>.</summary>
public struct WorldTransform : IEquatable<WorldTransform>
{
    public float4x4 Matrix;

    public WorldTransform() => Matrix = float4x4.identity;

    public readonly bool Equals(WorldTransform other) => Matrix.Equals(other.Matrix);

    public override readonly bool Equals(object? obj) => obj is WorldTransform other && Equals(other);

    public override readonly int GetHashCode() => Matrix.GetHashCode();

    public static bool operator ==(WorldTransform left, WorldTransform right) => left.Equals(right);

    public static bool operator !=(WorldTransform left, WorldTransform right) => !left.Equals(right);
}
