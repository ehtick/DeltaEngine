using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text.Json;
using DeltaEngine.Integration;
using DeltaEngine.Windowed;
using DeltaRender;
using Xunit;

namespace DeltaEngine.Windowed.Tests;

public sealed class WindowedCompositionTests
{
    private static readonly string[] ExpectedFrameOrder =
    [
        "input.init", "world.init", "render.init", "ui.init",
        "input.poll", "world.update", "ui.update", "render.frame"
    ];

    [Fact]
    public void SdfUniformsUseDeltaMathsWithoutOwningAClock()
    {
        var uniforms = FullscreenSdfShaderFixture.CreateUniforms(new EngineSurfaceSnapshot(800, 600));

        Assert.Equal(800, uniforms.Resolution.x);
        Assert.Equal(600, uniforms.Resolution.y);
        Assert.Equal(0, uniforms.TimeSeconds);
        Assert.Contains(nameof(IRenderWindowFrameSession.DrawFullscreenTriangle),
            typeof(IRenderWindowFrameSession).GetMethods().Select(static method => method.Name));
    }

    [Fact]
    public void HostOrdersPlatformPollWorldRenderAndUi()
    {
        var calls = new List<string>();
        using var input = new FakeInput(calls);
        using var world = new FakeWorld(calls);
        using var render = new FakeRender(calls);
        using var ui = new FakeUi(calls);
        using var host = new EngineHost(input, world, render, ui);

        host.Start();
        host.RunFrame(0.5f);

        Assert.Equal(ExpectedFrameOrder, calls);
        Assert.Equal(0, render.FrameNumber);
        Assert.Equal(new EngineSurfaceSnapshot(320, 200), render.Surface);
    }

    [Fact]
    public void RendererHasNoInputPollingHook()
    {
        var renderMethods = typeof(IEngineRenderService).GetMethods().Select(static method => method.Name).ToArray();

        Assert.DoesNotContain(nameof(IEngineInputService.PollInput), renderMethods);
    }

    [Fact]
    public void UiProviderSelectsGeneratedUiPairWithMatchingPushConstantMetadata()
    {
        var fullscreen = WindowShaderArtifactSelection.For(false);
        var ui = WindowShaderArtifactSelection.For(true);

        Assert.Equal("fullscreen-rounded-rectangle.vert", fullscreen.VertexName);
        Assert.Equal("fullscreen-rounded-rectangle.frag", fullscreen.FragmentName);
        Assert.Equal("ui-panel.vert", ui.VertexName);
        Assert.Equal("ui-panel.frag", ui.FragmentName);
        Assert.True(ui.UsesUiPushConstants);
        Assert.False(fullscreen.UsesUiPushConstants);

        var fullscreenVertex = ReadManifest(fullscreen.VertexName);
        var fullscreenFragment = ReadManifest(fullscreen.FragmentName);
        var uiVertex = ReadManifest(ui.VertexName);
        var uiFragment = ReadManifest(ui.FragmentName);

        Assert.Equal(DeltaShader.Abstractions.ShaderStage.Vertex, uiVertex.Stage);
        Assert.Equal(DeltaShader.Abstractions.ShaderStage.Fragment, uiFragment.Stage);
        Assert.Equal(uiVertex.PushConstants[0].Size, uiFragment.PushConstants[0].Size);
        Assert.True(fullscreenVertex.PushConstants.Count == 0);
        Assert.NotEqual(fullscreenFragment.PushConstants[0].Size, uiFragment.PushConstants[0].Size);
    }

    private static DeltaShader.Abstractions.ShaderAbiManifest ReadManifest(string shaderName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "shaders", shaderName + ".shader.json");
        return JsonSerializer.Deserialize<DeltaShader.Abstractions.ShaderAbiManifest>(File.ReadAllText(path))
            ?? throw new InvalidDataException(path);
    }

    private sealed class FakeInput(List<string> calls) : IEnginePlatformShell
    {
        public EngineSurfaceSnapshot Surface => new(320, 200);
        public void Initialize() => calls.Add("input.init");
        public InputSnapshot PollInput(int frameNumber, float deltaSeconds)
        {
            calls.Add("input.poll");
            return new InputSnapshot(frameNumber, Surface: Surface);
        }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeWorld(List<string> calls) : IEngineWorldService
    {
        public void Initialize() => calls.Add("world.init");
        public void Update(in EngineFrameContext context) => calls.Add("world.update");
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeRender(List<string> calls) : IEngineRenderService
    {
        public long FrameNumber { get; private set; }
        public EngineSurfaceSnapshot Surface { get; private set; }
        public void Initialize() => calls.Add("render.init");
        public void Render(in EngineRenderFrame frame)
        {
            calls.Add("render.frame");
            FrameNumber = frame.FrameNumber;
            Surface = frame.Surface;
        }
        public void Shutdown() { }
        public void Dispose() { }
    }

    private sealed class FakeUi(List<string> calls) : IEngineUiService
    {
        public void Initialize() => calls.Add("ui.init");
        public void Update(in EngineFrameContext context) => calls.Add("ui.update");
        public void Shutdown() { }
        public void Dispose() { }
    }
}
