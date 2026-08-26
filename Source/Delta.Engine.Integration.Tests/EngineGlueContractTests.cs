using Delta.Engine.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EngineGlueContractTests
{
    private static readonly EngineLifecycleStage[] ShutdownStages =
    [
        EngineLifecycleStage.FrameStarted,
        EngineLifecycleStage.InputPolled,
        EngineLifecycleStage.WorldUpdated,
        EngineLifecycleStage.UiUpdated,
        EngineLifecycleStage.RenderUpdated,
        EngineLifecycleStage.FrameCompleted,
        EngineLifecycleStage.ShutdownStarted,
        EngineLifecycleStage.UiShutdown,
        EngineLifecycleStage.RenderShutdown,
        EngineLifecycleStage.WorldShutdown,
        EngineLifecycleStage.InputShutdown,
    ];

    [Fact]
    public void RenderServiceReceivesCanonicalTimeFreeFrame()
    {
        using var render = new CapturingRenderService();
        var frame = new EngineRenderFrame(7, new EngineSurfaceSnapshot(1280, 720, IsResized: true));

        render.Initialize();
        render.Render(in frame);

        Assert.Equal(frame, render.LastFrame);
    }

    [Fact]
    public void FrameLoopStopsWhenPlatformRequestsExit()
    {
        using var input = new ExitAfterOneFrameInput();
        using var world = new NoopWorld();
        using var render = new NoopRender();
        using var ui = new NoopUi();
        using var host = new EngineHost(input, world, render, ui);
        using var loop = new EngineFrameLoop(host, new FixedClock());

        loop.Run();

        Assert.Equal(1, host.CompletedFrames);
        Assert.False(host.IsRunning);
    }

    [Fact]
    public void ExitShutdownHappensAfterTheCompletedFrame()
    {
        using var input = new ExitAfterOneFrameInput();
        using var world = new NoopWorld();
        using var render = new NoopRender();
        using var ui = new NoopUi();
        using var host = new EngineHost(input, world, render, ui);
        using var loop = new EngineFrameLoop(host, new FixedClock());

        loop.Run();

        Assert.Equal(ShutdownStages, host.StageLog.Skip(4).Select(static stage => stage.Stage));
    }

    private sealed class CapturingRenderService : IEngineRenderService
    {
        public EngineRenderFrame LastFrame { get; private set; }
        public void Initialize() { }
        public void Render(in EngineRenderFrame frame) => LastFrame = frame;
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class ExitAfterOneFrameInput : IEngineInputService
    {
        public void Initialize() { }

        public InputSnapshot PollInput(int frameNumber, float deltaSeconds) =>
            new(frameNumber, ExitRequested: true, Surface: new EngineSurfaceSnapshot(1, 1));

        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FixedClock : IEngineFrameClock
    {
        public float NextDeltaSeconds() => 1f / 60f;
    }

    private sealed class NoopWorld : IEngineWorldService
    {
        public void Initialize() { }
        public void Update(in EngineFrameContext context) { }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class NoopRender : IEngineRenderService
    {
        public void Initialize() { }
        public void Render(in EngineRenderFrame frame) { }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class NoopUi : IEngineUiService
    {
        public void Initialize() { }
        public void Update(in EngineFrameContext context) { }
        public void Shutdown() { }
        public void Dispose() { }
    }
}
