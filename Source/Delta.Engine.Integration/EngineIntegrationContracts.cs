using System;
using System.Collections.Generic;
using Delta.XAML.Contract;

namespace Delta.Engine.Integration;

public enum EngineLifecycleStage
{
    Unknown = 0,
    InputInitialized = 1,
    WorldInitialized = 2,
    RenderInitialized = 3,
    UiInitialized = 4,
    FrameStarted = 5,
    InputPolled = 6,
    WorldUpdated = 7,
    RenderUpdated = 8,
    UiUpdated = 9,
    FrameCompleted = 10,
    ShutdownStarted = 11,
    InputShutdown = 12,
    WorldShutdown = 13,
    RenderShutdown = 14,
    UiShutdown = 15,
    HostDisposalStarted = 16,
    UiDisposed = 17,
    RenderDisposed = 18,
    WorldDisposed = 19,
    InputDisposed = 20,
    HostDisposed = 21,
}

public readonly record struct EngineLifecycleEvent(EngineLifecycleStage Stage, int FrameNumber);

public readonly record struct EngineSurfaceSnapshot(int Width, int Height, bool IsResized = false)
{
    public static EngineSurfaceSnapshot Empty => default;

    public bool IsValid => Width > 0 && Height > 0;
}

public enum EngineInputEventKind
{
    Unknown,
    Quit,
    KeyDown,
    KeyUp,
    PointerMove,
    PointerDown,
    PointerUp,
}

public readonly record struct EngineInputEvent(
    EngineInputEventKind Kind,
    int Code = 0,
    float X = 0,
    float Y = 0);

public readonly record struct InputSnapshot(
    int FrameNumber,
    bool ExitRequested = false,
    EngineSurfaceSnapshot Surface = default,
    ReadOnlyMemory<EngineInputEvent> Events = default,
    ReadOnlyMemory<UiInputEvent> UiEvents = default);

public readonly record struct EngineFrameContext(
    int FrameNumber,
    float DeltaSeconds,
    InputSnapshot Input,
    float ElapsedSeconds = 0)
{
    public EngineSurfaceSnapshot Surface => Input.Surface;
}

public interface IEngineHost : IDisposable
{
    bool IsRunning { get; }
    bool IsDisposed { get; }
    int CompletedFrames { get; }

    IReadOnlyList<EngineLifecycleEvent> StageLog { get; }

    void Start();
    void RunFrame(float deltaSeconds);
    void Shutdown();
}

public interface IEngineInputService : IDisposable
{
    void Initialize();
    InputSnapshot PollInput(int frameNumber, float deltaSeconds);
    void Shutdown();
}

public interface IEnginePlatformShell : IEngineInputService
{
    EngineSurfaceSnapshot Surface { get; }
}

public interface IEngineWorldService : IDisposable
{
    void Initialize();
    void Update(in EngineFrameContext context);
    void Shutdown();
}

public interface IEngineRenderService : IDisposable
{
    void Initialize();

    // Render services receive identity and surface only. Time domains, input and
    // simulation state remain owned by the host and feature extractors.
    void Render(in EngineRenderFrame frame);
    void Shutdown();
}

public interface IEngineUiService : IDisposable
{
    void Initialize();
    void Update(in EngineFrameContext context);
    void Shutdown();
}

// Optional same-frame path. The provider owns UI mutations, styles, layout and
// extraction; EngineHost only schedules it before rendering.
public interface IEngineUiFrameSource : IEngineUiService
{
    void PrepareFrame(in EngineFrameContext context);
}

public readonly record struct EngineEntityId(uint Value)
{
    public bool IsValid => Value != 0;
}

public interface IEngineWorldAccess : IDisposable
{
    // Read-only access must not change the ECS consumer's dirty state.
    bool TryRead<T>(EngineEntityId entity, out T value) where T : struct;
    ReadOnlySpan<T> ReadAll<T>() where T : struct;

    // Implementations mark the addressed component/chunk dirty before exposing mutable storage.
    ref T GetMutable<T>(EngineEntityId entity) where T : struct;
    Span<T> GetMutableAll<T>() where T : struct;
}

public interface IEngineWorldConsumer
{
    void Update(IEngineWorldAccess world, in EngineFrameContext context);
}

public sealed class EngineWorldServiceAdapter(
    IEngineWorldAccess world,
    IEngineWorldConsumer consumer) : IEngineWorldService
{
    public void Initialize() { }

    public void Update(in EngineFrameContext context) => consumer.Update(world, context);

    public void Shutdown() { }

    public void Dispose() => world.Dispose();
}

public readonly record struct EngineShaderId(string Value);

public interface IEngineShaderModuleSource : IDisposable
{
    bool TryGetModule(EngineShaderId shader, out ReadOnlyMemory<byte> shaderModule);
}

public interface IEngineFrameClock
{
    float NextDeltaSeconds();
}

public sealed class EngineFrameLoop(IEngineHost host, IEngineFrameClock clock) : IDisposable
{
    public void Run()
    {
        host.Start();
        while (host.IsRunning)
        {
            host.RunFrame(clock.NextDeltaSeconds());
        }
    }

    public void Dispose() => host.Dispose();
}
