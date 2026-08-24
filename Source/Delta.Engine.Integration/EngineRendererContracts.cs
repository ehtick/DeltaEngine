namespace Delta.Engine.Integration;

/// <summary>
/// Time-free renderer input. Engine-owned clocks are extracted into explicit
/// feature or shader data before submission.
/// </summary>
public readonly record struct EngineRenderFrame(
    long FrameNumber,
    EngineSurfaceSnapshot Surface);

public interface IRenderFrameSink : IDisposable
{
    void Resize(in EngineSurfaceSnapshot surface);

    void Render(in EngineRenderFrame frame);
}

public interface IRenderer : IRenderFrameSink
{
}

/// <summary>
/// A deterministic backend-free renderer for headless engine/editor/game runs.
/// It records lifecycle observations for diagnostics but performs no rendering.
/// </summary>
public sealed class NullRenderer : IRenderer
{
    private bool _disposed;

    public string BackendName => "none";

    public bool HasBackend => false;

    public bool IsDisposed => _disposed;

    public EngineSurfaceSnapshot LastSurface { get; private set; }

    public EngineRenderFrame? LastFrame { get; private set; }

    public int ResizeCount { get; private set; }

    public int RenderCount { get; private set; }

    public void Resize(in EngineSurfaceSnapshot surface)
    {
        ThrowIfDisposed();
        LastSurface = surface;
        ResizeCount++;
    }

    public void Render(in EngineRenderFrame frame)
    {
        ThrowIfDisposed();
        LastFrame = frame;
        RenderCount++;
    }

    public void Dispose()
    {
        _disposed = true;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
