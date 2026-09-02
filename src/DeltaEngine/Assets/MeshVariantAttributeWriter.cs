using System;

namespace Delta.Engine.Assets;

internal static class MeshVariantAttributeWriter
{
    public static void Write(MeshData meshData, VertexAttribute vertexMask, int vertexSize, byte[] destination)
    {
        int offset = 0;
        foreach (var attribute in vertexMask.Iterate())
        {
            CopyAttribute(meshData, attribute.location, attribute.size, vertexSize, offset, destination);
            offset += attribute.size;
        }
    }

    private static void CopyAttribute(
        MeshData meshData,
        int location,
        int size,
        int vertexSize,
        int offset,
        byte[] destination)
    {
        var attributeArray = meshData.GetAttributeArray(location);
        for (int index = 0; index < meshData.vertexCount; index++)
        {
            var source = attributeArray.Slice(size * index, size);
            var target = new Span<byte>(destination, index * vertexSize + offset, size);
            source.CopyTo(target);
        }
    }
}
