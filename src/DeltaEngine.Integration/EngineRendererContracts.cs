namespace Delta.Engine.Integration;

/// <summary>
/// Time-free renderer input. Engine-owned clocks are extracted into explicit
/// feature or shader data before submission.
/// </summary>
public readonly record struct EngineRenderFrame(
    long FrameNumber,
    EngineSurfaceSnapshot Surface);

/// <summary>
/// A deterministic backend-free renderer for headless engine/editor/game runs.
/// It records lifecycle observations for diagnostics but performs no rendering.
/// </summary>
public sealed class NullRenderer : IEngineRenderService
{
    private bool _disposed;

    public string BackendName => "none";

    public bool HasBackend => false;

    public bool IsDisposed => _disposed;

    public EngineSurfaceSnapshot LastSurface { get; private set; }

    public EngineRenderFrame? LastFrame { get; private set; }

    public int ResizeCount { get; private set; }

    public int RenderCount { get; private set; }

    public void Initialize()
    {
        ThrowIfDisposed();
    }

    public void Render(in EngineRenderFrame frame)
    {
        ThrowIfDisposed();
        if (frame.Surface != LastSurface)
        {
            LastSurface = frame.Surface;
            ResizeCount++;
        }

        LastFrame = frame;
        RenderCount++;
    }

    public void Shutdown()
    {
        ThrowIfDisposed();
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
