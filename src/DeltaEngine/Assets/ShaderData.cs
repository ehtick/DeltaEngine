using System;
using System.Text.Json.Serialization;

namespace Delta.Engine.Assets;

public class ShaderData : IAsset
{
    public readonly VertexAttribute attributeMask;
    private readonly byte[] vert;
    private readonly byte[] frag;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "These methods expose borrowed spans over shader byte storage.")]
    public ReadOnlySpan<byte> GetVertBytes() => vert;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "These methods expose borrowed spans over shader byte storage.")]
    public ReadOnlySpan<byte> GetFragBytes() => frag;

    [JsonConstructor]
    public ShaderData(byte[] vert, byte[] frag, VertexAttribute attributeMask)
    {
        ArgumentNullException.ThrowIfNull(vert);
        ArgumentNullException.ThrowIfNull(frag);
        this.vert = (byte[])vert.Clone();
        this.frag = (byte[])frag.Clone();
        this.attributeMask = attributeMask;
    }
}
