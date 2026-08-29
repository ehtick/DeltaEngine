using System;
using System.Text.Json.Serialization;

namespace Delta.Engine.Assets;

public class MeshData : IAsset
{
    public readonly int vertexCount;
    private readonly byte[][] vertices;
    private readonly uint[] indices;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "These methods expose borrowed spans over GPU asset storage.")]
    public uint GetIndicesCount() => (uint)indices.Length;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "These methods expose borrowed spans over GPU asset storage.")]
    public ReadOnlySpan<uint> GetIndices() => indices;
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1024:Use properties where appropriate", Justification = "These methods expose borrowed spans over GPU asset storage.")]
    public ReadOnlySpan<byte> GetAttributeArray(int index) => vertices[index];

    [JsonConstructor]
    public MeshData(int vertexCount, uint[] indices, byte[][] vertices)
    {
        ArgumentNullException.ThrowIfNull(indices);
        ArgumentNullException.ThrowIfNull(vertices);
        this.vertexCount = vertexCount;
        this.indices = (uint[])indices.Clone();
        this.vertices = new byte[vertices.GetLength(0)][];
        for (int i = 0; i < vertices.GetLength(0); i++)
        {
            if (vertices[i] != null)
            {
                this.vertices[i] = (byte[])vertices[i].Clone();
            }
        }
    }

    public MeshData(int vertexCount, uint[] indices)
    {
        ArgumentNullException.ThrowIfNull(indices);
        this.vertexCount = vertexCount;
        this.indices = indices;
        vertices = new byte[16][];
    }

    public unsafe void SetData(VertexAttribute attribute, void* dataPointer)
    {
        if (dataPointer != null)
        {
            vertices[attribute.GetAttributeLocation()] = new Span<byte>(dataPointer, vertexCount * attribute.GetAttributeSize()).ToArray();
        }
    }
}
