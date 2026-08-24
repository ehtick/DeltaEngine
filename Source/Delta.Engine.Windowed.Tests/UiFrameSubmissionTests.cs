using System;
using System.Threading.Tasks;
using Delta.Render.Core;
using Delta.Shader.Abstractions;
using Delta.Engine.Windowed;
using Xunit;

namespace Delta.Engine.Windowed.Tests;

public sealed class UiFrameSubmissionTests
{
    [Fact]
    public async Task BorrowsOnceAndThrowsWhenEndFrameFails()
    {
        using var source = new FakeSource();
        await using var session = new FakeSession();
        var pipeline = new FakePipeline();
        var state = RenderFrameState.Ready(0, new WindowMetrics(320, 180, 1));
        var parameters = new GraphicsFrameParameters(320, 180, 0);

        Assert.Throws<InvalidOperationException>(() =>
            WindowedUiFrameSubmission.EndFrameOrThrow(session, source, in state, pipeline, in parameters));
        Assert.Equal(1, source.BorrowCount);
        Assert.Equal(1, session.EndFrameCount);
    }

    private sealed class FakeSource : IUiRenderFrameSource, IDisposable
    {
        private readonly UiRenderBatchAdapter _adapter = new();
        private readonly UiRenderFrameToken _token;

        public FakeSource() => _token = _adapter.Replace([new UiQuad(0, 0, 10, 10, 1, 1, 1, 1)], [], []);
        public int BorrowCount { get; private set; }
        public UiRenderFrameView BorrowFrame()
        {
            BorrowCount++;
            return new UiRenderFrameView(_adapter.Borrow(in _token), []);
        }
        public void Dispose() => _adapter.Dispose();
    }

    private sealed class FakePipeline : IGraphicsPipeline
    {
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }

    private sealed class FakeSession : IRenderWindowFrameSession
    {
        public int EndFrameCount { get; private set; }
        public RenderWindowId WindowId => RenderWindowId.New();
        public RenderFrameState BeginFrame() => RenderFrameState.Ready(0, new WindowMetrics(1, 1, 1));
        public bool EndFrame(in RenderFrameState frameState, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, ReadOnlySpan<UiQuad> uiQuads, ReadOnlySpan<RenderRecordChange> dirtyRecords) { EndFrameCount++; return false; }
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, ReadOnlySpan<UiQuad> uiQuads, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<ITextAtlasPage> atlasPages, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, ReadOnlySpan<UiQuad> uiQuads, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<TextGlyphInstance> textGlyphs, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, in UiDrawList drawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool SubmitFrame(IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, in UiDrawList drawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool SubmitFrame(IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, in UiDrawList uiDrawList, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<ITextAtlasPage> atlasPages, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool SubmitFrame(IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, in UiDrawList uiDrawList, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public IGraphicsPipeline CreateTextPipeline(in GraphicsShaderProgram shaderProgram) => throw new NotSupportedException();
        public IGraphicsPipeline CreateGraphicsPipeline(in GraphicsShaderProgram shaderProgram) => throw new NotSupportedException();
        public ITextAtlasDevice CreateTextAtlasDevice() => throw new NotSupportedException();
        public bool DrawFullscreenTriangle(IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters) => false;
        public bool Resize(WindowMetrics metrics) => false;
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
