using System.Collections.Generic;
using Delta.Engine.Integration;
using Delta.Engine.Windowed;
using Xunit;

namespace Delta.Engine.Windowed.Tests;

public sealed class WindowedCompositionTests
{
    private static readonly string[] ExpectedFrameOrder = ["input.poll", "world.update", "ui.update", "render"];

    [Fact]
    public void ShaderSelectionSeparatesFullscreenAndUiPrograms()
    {
        var fullscreen = WindowShaderArtifactSelection.For(false);
        var ui = WindowShaderArtifactSelection.For(true);

        Assert.False(fullscreen.UsesUiPushConstants);
        Assert.True(ui.UsesUiPushConstants);
        Assert.NotEqual(fullscreen.VertexName, ui.VertexName);
        Assert.NotEqual(fullscreen.FragmentName, ui.FragmentName);
    }

    [Fact]
    public void HeadlessHostUsesInputWorldUiRenderOrder()
    {
        var calls = new List<string>();
        using var input = new FakeInput(calls);
        using var world = new FakeWorld(calls);
        using var render = new FakeRender(calls);
        using var ui = new FakeUi(calls);
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.RunFrame(0.016f);

        Assert.Equal(ExpectedFrameOrder, calls);
    }

    private sealed class FakeInput(List<string> calls) : IEngineInputService
    {
        public void Initialize() { }
        public InputSnapshot PollInput(int frameNumber, float deltaSeconds) { calls.Add("input.poll"); return new(frameNumber, Surface: new(320, 180)); }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeWorld(List<string> calls) : IEngineWorldService
    {
        public void Initialize() { }
        public void Update(in EngineFrameContext context) => calls.Add("world.update");
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeRender(List<string> calls) : IEngineRenderService
    {
        public void Initialize() { }
        public void Render(in EngineRenderFrame frame) => calls.Add("render");
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeUi(List<string> calls) : IEngineUiService
    {
        public void Initialize() { }
        public void Update(in EngineFrameContext context) => calls.Add("ui.update");
        public void Shutdown() { }
        public void Dispose() { }
    }
}
