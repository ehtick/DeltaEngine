using System;
using System.Collections.Generic;

namespace Delta.Engine.Assets;

internal sealed class MeshVariantCache
{
    private readonly Dictionary<Guid, Dictionary<VertexAttribute, WeakReference<byte[]?>>> _variants = [];

    public byte[] GetMeshVariant(Guid guid, VertexAttribute vertexMask, MeshData meshData)
    {
        if (!_variants.TryGetValue(guid, out var meshVariants))
        {
            _variants[guid] = meshVariants = [];
        }

        if (!meshVariants.TryGetValue(vertexMask, out var reference))
        {
            meshVariants[vertexMask] = reference = new(null);
        }

        if (!reference.TryGetTarget(out var result))
        {
            reference.SetTarget(result = MeshVariantPacker.Pack(meshData, vertexMask));
        }

        return result;
    }
}
