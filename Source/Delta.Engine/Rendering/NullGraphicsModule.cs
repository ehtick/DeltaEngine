using System;
using Delta.Engine.Integration;

namespace Delta.Engine.Rendering;

public sealed class NullGraphicsModule : Delta.Engine.Runtime.IGraphicsModule
{
    private readonly IRenderer _renderer;
    private bool _disposed;
    private long _frameNumber;
    private (int width, int height) _size;

    public NullGraphicsModule(string appName, IRenderer? renderer = null)
    {
        _ = appName;
        _renderer = renderer ?? new NullRenderer();
    }

    public IRenderer Renderer => _renderer;

    public (int width, int height) Size
    {
        get => _size;
        set => Resize(value.width, value.height);
    }

    public void Resize(int width, int height)
    {
        ThrowIfDisposed();
        _size = (width, height);
        var surface = new EngineSurfaceSnapshot(width, height, IsResized: true);
        _renderer.Resize(in surface);
    }

    public void Execute()
    {
        ThrowIfDisposed();
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

        _disposed = true;
        _renderer.Dispose();
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
