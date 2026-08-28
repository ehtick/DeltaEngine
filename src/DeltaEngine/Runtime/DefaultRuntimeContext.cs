using System;

namespace Delta.Engine.Runtime;

internal sealed record DefaultRuntimeContext(
    IProjectPath ProjectPath,
    IAssetCollection AssetImporter,
    ISceneManager SceneManager,
    IGraphicsModule GraphicsModule)
    : IRuntimeContext
{
    public bool Running { get; set; }
    public IRuntimeContext? PreviousContext { get; set; }

    public void Dispose()
    {
        GraphicsModule.Dispose();
        if (SceneManager is IDisposable disposableSceneManager)
        {
            disposableSceneManager.Dispose();
        }

        GC.SuppressFinalize(this);
        IRuntimeContext.RestoreCurrent(this);
    }
}

