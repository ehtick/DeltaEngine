using Delta.Engine.Integration;
using Delta.Engine.Windowed;
using Delta.Render;
using Delta.Render.RenderGraph;
using Xunit;

namespace Delta.Engine.Windowed.Tests;

public sealed class UiFrameSubmissionTests
{
    [Fact]
    public void InvalidSurfaceIsNotSubmittedByHeadlessRenderer()
    {
        using var renderer = new NullRenderer();
        renderer.Initialize();
        renderer.Render(new EngineRenderFrame(1, EngineSurfaceSnapshot.Empty));

        Assert.Equal(1, renderer.RenderCount);
        Assert.Equal(EngineSurfaceSnapshot.Empty, renderer.LastSurface);
    }

    [Fact]
    public void CanonicalRenderTargetRequiresValidHandle()
    {
        var target = new RenderTargetHandle(1, 1);

        Assert.True(target.IsValid);
    }

    [Fact]
    public void FullscreenUniformFixtureUsesCurrentSurface()
    {
        var uniforms = FullscreenSdfShaderFixture.CreateUniforms(new EngineSurfaceSnapshot(800, 450));

        Assert.Equal(800, uniforms.Resolution.x);
        Assert.Equal(450, uniforms.Resolution.y);
    }
}
