using System;

namespace Delta.Engine.Assets;

internal static class MeshVariantPacker
{
    public static byte[] Pack(MeshData meshData, VertexAttribute vertexMask)
    {
        int vertexSize = vertexMask.GetVertexSize();
        var meshSize = vertexSize * meshData.vertexCount;
        byte[] result = new byte[meshSize];
        MeshVariantAttributeWriter.Write(meshData, vertexMask, vertexSize, result);
        return result;
    }
}
