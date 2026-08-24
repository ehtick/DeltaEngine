using Delta.Engine.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EngineHostContractTests
{
    [Fact]
    public void StartRecordsDeterministicInitializationOrder()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();

        var stages = host.StageLog.Select(s => s.Stage).ToArray();
        Assert.Equal(
            new[]
            {
                EngineLifecycleStage.InputInitialized,
                EngineLifecycleStage.WorldInitialized,
                EngineLifecycleStage.RenderInitialized,
                EngineLifecycleStage.UiInitialized,
            },
            stages);
    }

    [Fact]
    public void StartIsIdempotent()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.Start();

        Assert.Equal(4, host.StageLog.Count);
    }

    [Fact]
    public void RunFrameRecordsDeterministicStageOrder()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.RunFrame(0.016f);

        var stages = host.StageLog.Select(s => s.Stage).ToArray();
        Assert.Equal(
            new[]
            {
                EngineLifecycleStage.InputInitialized,
                EngineLifecycleStage.WorldInitialized,
                EngineLifecycleStage.RenderInitialized,
                EngineLifecycleStage.UiInitialized,
                EngineLifecycleStage.FrameStarted,
                EngineLifecycleStage.InputPolled,
                EngineLifecycleStage.WorldUpdated,
                EngineLifecycleStage.RenderUpdated,
                EngineLifecycleStage.UiUpdated,
                EngineLifecycleStage.FrameCompleted,
            },
            stages);
    }

    [Fact]
    public void ShutdownRecordsExpectedOrder()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.Shutdown();

        var stages = host.StageLog.Select(s => s.Stage).ToArray();
        Assert.Equal(
            new[]
            {
                EngineLifecycleStage.InputInitialized,
                EngineLifecycleStage.WorldInitialized,
                EngineLifecycleStage.RenderInitialized,
                EngineLifecycleStage.UiInitialized,
                EngineLifecycleStage.ShutdownStarted,
                EngineLifecycleStage.InputShutdown,
                EngineLifecycleStage.WorldShutdown,
                EngineLifecycleStage.RenderShutdown,
                EngineLifecycleStage.UiShutdown,
            },
            stages);
    }

    [Fact]
    public void RunFramePropagatesServiceExceptionWithoutSwallowing()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService { ThrowOnUpdate = true };
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        var exception = Assert.Throws<InvalidOperationException>(() => host.RunFrame(0.016f));

        Assert.Equal("World update failure", exception.Message);
        Assert.Equal(0, host.CompletedFrames);
        Assert.Equal(
            new[]
            {
                EngineLifecycleStage.InputInitialized,
                EngineLifecycleStage.WorldInitialized,
                EngineLifecycleStage.RenderInitialized,
                EngineLifecycleStage.UiInitialized,
                EngineLifecycleStage.FrameStarted,
                EngineLifecycleStage.InputPolled,
                EngineLifecycleStage.WorldUpdated,
            },
            host.StageLog.Select(s => s.Stage).ToArray());
    }

    [Fact]
    public void ShutdownIsIdempotent()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.Shutdown();
        var stageCount = host.StageLog.Count;

        host.Shutdown();

        Assert.Equal(stageCount, host.StageLog.Count);
        Assert.False(host.IsRunning);
    }

    [Fact]
    public void RunFrameRejectsInvalidDeltaTime()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);
        host.Start();

        Assert.Throws<ArgumentOutOfRangeException>(() => host.RunFrame(float.NaN));
        Assert.Equal(4, host.StageLog.Count);
    }

    [Fact]
    public void DisposeRecordsShutdownAndDisposalOrder()
    {
        using var input = new FakeInputService();
        using var world = new FakeWorldService();
        using var render = new FakeRenderService();
        using var ui = new FakeUiService();
        using var host = new EngineHost(input, world, render, ui);
        host.Start();
        host.Dispose();
        var stageCount = host.StageLog.Count;
        host.Dispose();

        Assert.Equal(stageCount, host.StageLog.Count);
        var stages = host.StageLog.Select(s => s.Stage).ToArray();
        Assert.Equal(
            new[]
            {
                EngineLifecycleStage.InputInitialized,
                EngineLifecycleStage.WorldInitialized,
                EngineLifecycleStage.RenderInitialized,
                EngineLifecycleStage.UiInitialized,
                EngineLifecycleStage.ShutdownStarted,
                EngineLifecycleStage.InputShutdown,
                EngineLifecycleStage.WorldShutdown,
                EngineLifecycleStage.RenderShutdown,
                EngineLifecycleStage.UiShutdown,
                EngineLifecycleStage.HostDisposalStarted,
                EngineLifecycleStage.UiDisposed,
                EngineLifecycleStage.RenderDisposed,
                EngineLifecycleStage.WorldDisposed,
                EngineLifecycleStage.InputDisposed,
                EngineLifecycleStage.HostDisposed,
            },
            stages);
    }

    private sealed class FakeInputService : IEngineInputService
    {
        public void Initialize() { }

        public InputSnapshot PollInput(int frameNumber, float deltaSeconds)
        {
            return new InputSnapshot(frameNumber);
        }

        public void Shutdown() { }

        public void Dispose() { }
    }

    private sealed class FakeWorldService : IEngineWorldService
    {
        public bool ThrowOnUpdate { get; init; }

        public void Initialize() { }

        public void Update(in EngineFrameContext context)
        {
            if (ThrowOnUpdate)
            {
                throw new InvalidOperationException("World update failure");
            }
        }

        public void Shutdown() { }

        public void Dispose() { }
    }

    private sealed class FakeRenderService : IEngineRenderService
    {
        public void Initialize() { }

        public void Render(in EngineFrameContext context) { }

        public void Shutdown() { }

        public void Dispose() { }
    }

    private sealed class FakeUiService : IEngineUiService
    {
        public void Initialize() { }

        public void Update(in EngineFrameContext context) { }

        public void Shutdown() { }

        public void Dispose() { }
    }
}
