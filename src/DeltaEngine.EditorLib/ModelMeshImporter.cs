using Delta;
using Silk.NET.Assimp;
using System;

namespace Delta.Engine.Assets;

internal static unsafe class ModelMeshImporter
{
    public static (MeshData data, string name) Read(Mesh* mesh)
    {
        var vertexCount = (int)mesh->MNumVertices;
        var meshData = new MeshData(vertexCount, ModelMeshIndices.Read(mesh));
        var (positions, vertices2) = ModelMeshVertices.Read(mesh, vertexCount);
        ModelMeshAttributes.Write(meshData, mesh, positions, vertices2, vertexCount);
        return (meshData, mesh->MName);
    }
}
