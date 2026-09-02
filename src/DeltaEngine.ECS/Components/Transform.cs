using Delta.Maths;

namespace Delta.Engine.ECS.Components;

/// <summary>Local position, rotation and scale stored as a DeltaECS component.</summary>
public struct Transform : IEquatable<Transform>
{
    public float3 position;
    public quaternion rotation;
    public float3 scale;

    public Transform()
    {
        position = float3.zero;
        rotation = quaternion.identity;
        scale = new float3(1);
    }

    public readonly float4x4 LocalMatrix => float4x4.CreateTRS(position, rotation, scale);

    public readonly bool Equals(Transform other) => position.Equals(other.position)
        && rotation.Equals(other.rotation)
        && scale.Equals(other.scale);

    public override readonly bool Equals(object? obj) => obj is Transform other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(position, rotation, scale);

    public static bool operator ==(Transform left, Transform right) => left.Equals(right);

    public static bool operator !=(Transform left, Transform right) => !left.Equals(right);
}
