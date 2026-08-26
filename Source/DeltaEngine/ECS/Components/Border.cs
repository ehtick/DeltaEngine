using Delta.Engine.Assets;
using Delta.Engine.ECS.Attributes;
using Delta.Maths;
using System;
namespace Delta.Engine.ECS.Components;

[Component]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1051:Do not declare visible instance fields",
    Justification = "ECS component fields are public by design for generated ref access and blittable layout.")]
public struct Border : IEquatable<Border>
{
    public float4 minMax;
    public float4 uv;
    public float4 margin;
    public float4 padding;
    //public Color colors;
    //public Color borderColors;
    public float4 cornerRadius;
    public int borderThickness;
    public GuidAsset<ShaderData> shader;

    public Border()
    {
        minMax = new(-1, -1, 1, 1);
    }

    public readonly bool Equals(Border other) =>
        minMax.Equals(other.minMax) && uv.Equals(other.uv) && margin.Equals(other.margin) &&
        padding.Equals(other.padding) && cornerRadius.Equals(other.cornerRadius) &&
        borderThickness == other.borderThickness && shader.Equals(other.shader);

    public override readonly bool Equals(object? obj) => obj is Border other && Equals(other);

    public override readonly int GetHashCode() => HashCode.Combine(minMax, uv, margin, padding, cornerRadius, borderThickness, shader);

    public static bool operator ==(Border left, Border right) => left.Equals(right);

    public static bool operator !=(Border left, Border right) => !left.Equals(right);
}
