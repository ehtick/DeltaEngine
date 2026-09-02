using System;

namespace Delta.Engine.Assets;

internal static class MeshVariantPacker
{
    public static byte[] Pack(MeshData meshData, VertexAttribute vertexMask)
    {
        int vertexSize = vertexMask.GetVertexSize();
        var meshSize = vertexSize * meshData.vertexCount;
        byte[] result = new byte[meshSize];
        int offset = 0;
        foreach (var attrib in vertexMask.Iterate())
        {
            var attribArray = meshData.GetAttributeArray(attrib.location);
            int attribSize = attrib.size;
            for (int i = 0; i < meshData.vertexCount; i++)
            {
                var source = attribArray.Slice(attribSize * i, attribSize);
                var destination = new Span<byte>(result, i * vertexSize + offset, attribSize);
                source.CopyTo(destination);
            }
            offset += attribSize;
        }
        return result;
    }
}
