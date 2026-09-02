using Delta.Engine.Runtime;
using Silk.NET.Assimp;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using Scene = Silk.NET.Assimp.Scene;

namespace Delta.Engine.Assets;

public sealed class ModelImporter : IDisposable
{
    private static readonly Assimp _assimp = Assimp.GetApi();
    public ImmutableHashSet<string> FileFormats { get; } = ["fbx"];

    public void Dispose()
    {
        _assimp.Dispose();
        GC.SuppressFinalize(this);
    }

    public unsafe void Import(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        var meshDatas = ModelImportWorkflow.Read(_assimp, path);
        foreach (var (meshData, name) in meshDatas)
        {
            IRuntimeContext.Current.AssetImporter.CreateAsset(meshData, $"{fileName}.{name}.mesh");
        }
    }

    public static unsafe IReadOnlyList<(MeshData meshData, string name)> ImportAndGet(string path) =>
        ModelImportWorkflow.Read(_assimp, path);
}
