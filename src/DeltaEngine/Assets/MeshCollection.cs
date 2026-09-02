using System;
namespace Delta.Engine.Assets;

internal sealed class MeshCollection : DefaultAssetCollection<MeshData>
{
    private readonly MeshVariantCache _meshMapVariants = new();

    public unsafe byte[] GetMeshVariant(VertexAttribute vertexMask, Guid guid)
        => _meshMapVariants.GetMeshVariant(guid, vertexMask, GetAsset(new GuidAsset<MeshData>(guid)));

    public static byte[] GetMeshVariant(MeshData meshData, VertexAttribute vertexMask)
        => MeshVariantPacker.Pack(meshData, vertexMask);
}
