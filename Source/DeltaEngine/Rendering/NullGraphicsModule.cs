using System;
using Delta.Engine.Integration;

namespace Delta.Engine.Rendering;

public sealed class NullGraphicsModule : Delta.Engine.Runtime.IGraphicsModule
{
    private readonly IEngineRenderService _renderer;
    private bool _disposed;
    private bool _rendererInitialized;
    private long _frameNumber;
    private (int width, int height) _size;

    public NullGraphicsModule(string appName, IEngineRenderService? renderer = null)
    {
        _ = appName;
        _renderer = renderer ?? new NullRenderer();
    }

    public IEngineRenderService Renderer => _renderer;

    public (int width, int height) Size
    {
        get => _size;
        set => Resize(value.width, value.height);
    }

    public void Resize(int width, int height)
    {
        ThrowIfDisposed();
        EnsureRendererInitialized();
        _size = (width, height);
        var surface = new EngineSurfaceSnapshot(width, height, IsResized: true);
        _renderer.Render(new EngineRenderFrame(_frameNumber++, surface));
    }

    public void Execute()
    {
        ThrowIfDisposed();
        EnsureRendererInitialized();
        var surface = new EngineSurfaceSnapshot(_size.width, _size.height, IsResized: true);
        var frame = new EngineRenderFrame(_frameNumber++, surface);
        _renderer.Render(in frame);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        try
        {
            if (_rendererInitialized)
            {
                _renderer.Shutdown();
            }
        }
        finally
        {
            _renderer.Dispose();
            _disposed = true;
        }
    }

    private void EnsureRendererInitialized()
    {
        if (_rendererInitialized)
        {
            return;
        }

        _renderer.Initialize();
        _rendererInitialized = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
