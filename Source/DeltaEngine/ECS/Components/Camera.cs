using DeltaEngine.ECS.Attributes;
using System;

namespace DeltaEngine.ECS.Components;

[Component(0)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1051:Do not declare visible instance fields",
    Justification = "ECS component fields are public by design for generated ref access and blittable layout.")]
public struct Camera : IEquatable<Camera>
{
    public float fieldOfView;
    public float aspectRation;
    public float nearPlaneDistance;
    public float farPlaneDistance;

    public Camera()
    {
        fieldOfView = 90;
        aspectRation = 1;
        nearPlaneDistance = 0;
        farPlaneDistance = 1000;
    }

    public readonly bool Equals(Camera other) => fieldOfView == other.fieldOfView &&
        aspectRation == other.aspectRation && nearPlaneDistance == other.nearPlaneDistance &&
        farPlaneDistance == other.farPlaneDistance;

    public override readonly bool Equals(object? obj) => obj is Camera other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(fieldOfView, aspectRation, nearPlaneDistance, farPlaneDistance);

    public static bool operator ==(Camera left, Camera right) => left.Equals(right);

    public static bool operator !=(Camera left, Camera right) => !left.Equals(right);
}
