using DeltaEngine.Integration;
using Xunit;

namespace DeltaEngine.Integration.Tests;

public sealed class UiFrameSourceContractTests
{
    private static readonly string[] ExpectedEvents = ["ui", "render"];

    [Fact]
    public void OptionalUiFrameSourceIsPreparedBeforeRender()
    {
        var events = new List<string>();
        using var input = new FakeInput();
        using var world = new FakeWorld();
        using var ui = new FakeUi(events);
        using var render = new FakeRender(events);
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.RunFrame(0.016f);

        Assert.Equal(ExpectedEvents, events);
    }

    private sealed class FakeInput : IEngineInputService
    {
        public void Initialize() { }
        public InputSnapshot PollInput(int frameNumber, float deltaSeconds) => new(frameNumber);
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeWorld : IEngineWorldService
    {
        public void Initialize() { }
        public void Update(in EngineFrameContext context) { }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeUi(List<string> events) : IEngineUiFrameSource
    {
        public void Initialize() { }
        public void PrepareFrame(in EngineFrameContext context) => events.Add("ui");
        public void Update(in EngineFrameContext context) => throw new InvalidOperationException("Host should use PrepareFrame.");
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeRender(List<string> events) : IEngineRenderService
    {
        public void Initialize() { }
        public void Render(in EngineRenderFrame frame) => events.Add("render");
        public void Shutdown() { }
        public void Dispose() { }
    }
}
