using Delta.Engine.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EngineGlueContractTests
{
    private static readonly string[] ResizeThenRender = ["initialize", "resize", "render"];
    private static readonly string[] ResizeOnceThenRender = ["initialize", "resize", "render", "render"];
    private static readonly EngineLifecycleStage[] ShutdownStages =
    [
        EngineLifecycleStage.FrameStarted,
        EngineLifecycleStage.InputPolled,
        EngineLifecycleStage.WorldUpdated,
        EngineLifecycleStage.RenderUpdated,
        EngineLifecycleStage.UiUpdated,
        EngineLifecycleStage.FrameCompleted,
        EngineLifecycleStage.ShutdownStarted,
        EngineLifecycleStage.InputShutdown,
        EngineLifecycleStage.WorldShutdown,
        EngineLifecycleStage.RenderShutdown,
        EngineLifecycleStage.UiShutdown,
    ];

    [Fact]
    public void Render_adapter_forwards_resize_before_frame()
    {
        using var sink = new FakeRenderSink();
        using var adapter = new EngineRenderServiceAdapter(sink);
        var input = new InputSnapshot(7, Surface: new EngineSurfaceSnapshot(1280, 720));

        adapter.Initialize();
        adapter.Render(new EngineFrameContext(7, 1f / 60f, input));

        Assert.Equal(ResizeThenRender, sink.Calls);
    }

    [Fact]
    public void RenderAdapterOnlyResizesWhenSurfaceChanges()
    {
        using var sink = new FakeRenderSink();
        using var adapter = new EngineRenderServiceAdapter(sink);
        var input = new InputSnapshot(7, Surface: new EngineSurfaceSnapshot(1280, 720));

        adapter.Initialize();
        adapter.Render(new EngineFrameContext(7, 1f / 60f, input));
        adapter.Render(new EngineFrameContext(8, 1f / 60f, input));

        Assert.Equal(ResizeOnceThenRender, sink.Calls);
    }

    [Fact]
    public void Frame_loop_stops_when_platform_requests_exit()
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

    private sealed class FakeRenderSink : IEngineRenderFrameSink
    {
        public List<string> Calls { get; } = [];

        public void Initialize() => Calls.Add("initialize");
        public void Resize(EngineSurfaceSnapshot surface) => Calls.Add("resize");
        public void Render(in EngineFrameContext context) => Calls.Add("render");
        public void Shutdown() => Calls.Add("shutdown");
        public void Dispose() => Calls.Add("dispose");
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
        public void Render(in EngineFrameContext context) { }
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
