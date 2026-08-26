using Arch.Core;
using System;
using Schedulers;

namespace Delta.Engine.Runtime;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1724:Type names should not conflict with namespaces",
    Justification = "Runtime is the established public engine host type.")]
public sealed class Runtime : IRuntime, IDisposable
{
    public IRuntimeContext Context { get; }
    private bool _disposed;

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "DefaultRuntimeContext takes ownership of the scene manager and graphics module and disposes both.")]
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
        if (_disposed)
        {
            return;
        }

        Context.Dispose();
        World.SharedJobScheduler?.Dispose();
        World.SharedJobScheduler = null;
        _disposed = true;
    }
}
