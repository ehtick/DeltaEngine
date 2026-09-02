using Silk.NET.Assimp;
using System;

namespace Delta.Engine.Assets;

internal static class ModelMeshIndices
{
    public static unsafe uint[] Read(Mesh* mesh)
    {
        Span<uint> indices = stackalloc uint[(int)mesh->MNumFaces * 3];
        var index = 0;
        for (uint face = 0; face < mesh->MNumFaces; face++)
        {
            int count = (int)mesh->MFaces[face].MNumIndices;
            if (count != 3)
            {
                continue;
            }

            new Span<uint>(mesh->MFaces[face].MIndices, count).CopyTo(indices[index..]);
            index += count;
        }

        return indices.ToArray();
    }
}
