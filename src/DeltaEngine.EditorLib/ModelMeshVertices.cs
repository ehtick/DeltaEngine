using Delta.Maths;
using Silk.NET.Assimp;

namespace Delta.Engine.Assets;

internal static class ModelMeshVertices
{
    public static unsafe (float3[] positions, float2[] vertices2) Read(Mesh* mesh, int vertexCount)
    {
        var positions = new float3[vertexCount];
        var vertices2 = new float2[vertexCount];
        for (var i = 0; i < vertexCount; i++)
        {
            var source = mesh->MVertices[i];
            positions[i] = new float3(source.X, source.Y, source.Z);
            vertices2[i] = new float2(source.X, source.Y);
        }

        return (positions, vertices2);
    }
}
