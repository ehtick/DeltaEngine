using Delta;
using Silk.NET.Assimp;

namespace Delta.Engine.Assets;

internal static class ModelMeshGeometryWriter
{
    public static unsafe void Write(MeshData meshData, float3[] positions, float2[] vertices2)
    {
        fixed (float3* position = positions)
        {
            meshData.SetData(VertexAttribute.Pos3, position);
        }

        fixed (float2* vertex = vertices2)
        {
            meshData.SetData(VertexAttribute.Pos2, vertex);
        }
    }
}
