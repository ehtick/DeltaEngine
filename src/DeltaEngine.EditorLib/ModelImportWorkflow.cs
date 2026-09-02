using Silk.NET.Assimp;
using System.Collections.Generic;
using Scene = Silk.NET.Assimp.Scene;

namespace Delta.Engine.Assets;

internal static unsafe class ModelImportWorkflow
{
    private const PostProcessSteps ImportMode = PostProcessSteps.Triangulate | PostProcessSteps.GenerateNormals | PostProcessSteps.JoinIdenticalVertices;

    public static IReadOnlyList<(MeshData meshData, string name)> Read(Assimp assimp, string path)
    {
        Scene* scene = assimp.ImportFile(path, (uint)ImportMode);
        List<(MeshData meshData, string name)> meshDatas = [];
        for (var i = 0; i < scene->MNumMeshes; i++)
        {
            meshDatas.Add(ModelMeshImporter.Read(scene->MMeshes[i]));
        }

        return meshDatas;
    }
}
