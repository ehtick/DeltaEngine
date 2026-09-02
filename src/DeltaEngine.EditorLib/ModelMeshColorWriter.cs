using Delta.Maths;
using Silk.NET.Assimp;
using System;

namespace Delta.Engine.Assets;

internal static class ModelMeshColorWriter
{
    public static unsafe void Write(MeshData meshData, Mesh* mesh, int vertexCount)
    {
        meshData.SetData(VertexAttribute.Norm, mesh->MNormals);
        meshData.SetData(VertexAttribute.Bitan, mesh->MBitangents);
        meshData.SetData(VertexAttribute.Tan, mesh->MTangents);
        if (mesh->MColors[0] != null)
        {
            meshData.SetData(VertexAttribute.Col, mesh->MColors[0]);
            return;
        }

        var white = new float4[vertexCount];
        Array.Fill(white, new float4(1, 1, 1, 1));
        fixed (float4* color = white)
        {
            meshData.SetData(VertexAttribute.Col, color);
        }
    }
}
