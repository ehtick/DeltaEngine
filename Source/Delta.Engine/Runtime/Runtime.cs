using Arch.Core;
using Delta.Engine.ECS;
using System;
using Schedulers;

namespace Delta.Engine.Runtime;

public sealed class Runtime : IRuntime, IDisposable
{
    public IRuntimeContext Context { get; }
    private bool _disposed;

    public Runtime(IProjectPath projectPath)
    {
        var path = projectPath;
        var assets = new GlobalAssetCollection();
        var sceneManager = new SceneManager();
        Context = new DefaultRuntimeContext(path, assets, sceneManager, new Rendering.NullGraphicsModule("Delta Editor"));
        IRuntimeContext.Current = Context;

    }
    public Runtime(IRuntimeContext context)
    {
        Context = context;
        IRuntimeContext.Current = Context;
    }

    public void Run()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        World.SharedJobScheduler ??= new JobScheduler(new JobScheduler.Config()
        {
            ThreadPrefixName = "Arch.Multithreading",
        });
        IRuntimeContext.Current.SceneManager.CurrentScene.Run();
        IRuntimeContext.Current.GraphicsModule.Execute();
        DestroySystem.Execute();
        DirtyFlagClearSystem.Execute();
    }

    public void Dispose()
    {
        World.SharedJobScheduler?.Dispose();
        World.SharedJobScheduler = null;
        _disposed = true;
    }
}
