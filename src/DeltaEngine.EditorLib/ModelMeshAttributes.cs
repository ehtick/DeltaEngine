using Delta.Maths;
using Silk.NET.Assimp;
using System;

namespace Delta.Engine.Assets;

internal static class ModelMeshAttributes
{
    public static unsafe void Write(MeshData meshData, Mesh* mesh, float3[] positions, float2[] vertices2, int vertexCount)
    {
        ModelMeshGeometryWriter.Write(meshData, positions, vertices2);
        ModelMeshColorWriter.Write(meshData, mesh, vertexCount);
    }
}
