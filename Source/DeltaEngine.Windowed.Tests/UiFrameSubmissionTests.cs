using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DeltaRender;
using DeltaShader.Abstractions;
using DeltaEngine.Windowed;
using Xunit;

namespace DeltaEngine.Windowed.Tests;

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
        TextGlyphInstance[] glyphScratch = [];
        TextBatchRange[] batchScratch = [];

        Assert.Throws<InvalidOperationException>(() =>
            WindowedUiFrameSubmission.EndFrameOrThrow(
                session,
                source,
                in state,
                pipeline,
                null,
                in parameters,
                ref glyphScratch,
                ref batchScratch));
        Assert.Equal(1, source.BorrowCount);
        Assert.Equal(1, session.EndFrameCount);
        Assert.Equal(0, session.SubmitFrameCount);
    }

    [Fact]
    public async Task TextWithoutPipelineFailsWithoutSubmitting()
    {
        using var source = new FakeSource(withText: true);
        await using var session = new FakeSession();
        var pipeline = new FakePipeline();
        var state = RenderFrameState.Ready(0, new WindowMetrics(320, 180, 1));
        var parameters = new GraphicsFrameParameters(320, 180, 0);
        TextGlyphInstance[] glyphScratch = [];
        TextBatchRange[] batchScratch = [];

        Assert.Throws<InvalidOperationException>(() =>
            WindowedUiFrameSubmission.EndFrameOrThrow(
                session,
                source,
                in state,
                pipeline,
                null,
                in parameters,
                ref glyphScratch,
                ref batchScratch));
        Assert.Equal(1, source.BorrowCount);
        Assert.Equal(0, session.EndFrameCount);
        Assert.Equal(0, session.SubmitFrameCount);
        Assert.Equal(4, glyphScratch.Length);
        Assert.Equal(4, batchScratch.Length);
    }

    [Fact]
    public async Task PreparedTextUsesOneBeginAndCombinedEnd()
    {
        using var source = new FakeSource(withText: true);
        await using var session = new FakeSession();
        var uiPipeline = new FakePipeline();
        var textPipeline = new FakePipeline();
        var state = session.BeginFrame();
        var parameters = new GraphicsFrameParameters(320, 180, 0);
        TextGlyphInstance[] glyphScratch = [];
        TextBatchRange[] batchScratch = [];

        WindowedUiFrameSubmission.EndFrameOrThrow(
            session,
            source,
            in state,
            uiPipeline,
            textPipeline,
            in parameters,
            ref glyphScratch,
            ref batchScratch);

        Assert.Equal(1, source.BorrowCount);
        Assert.Equal(1, session.BeginFrameCount);
        Assert.Equal(1, session.CombinedEndFrameCount);
        Assert.True(session.TextPipelineSeen);
        Assert.Equal(0, session.SubmitFrameCount);
    }

    [Fact]
    public void BorrowedServiceDoesNotDisposeBorrowedSession()
    {
        var session = new FakeSession();
        var platform = new Sdl3PlatformShell(new NullWindowFactory(), new WindowConfiguration("test"));
        using var service = new VulkanWindowRenderService(platform, session);

        service.Dispose();

        Assert.Equal(0, session.DisposeCount);
    }

    [Fact]
    public void OwnedServiceDisposesPipelineBeforeOwnedSession()
    {
        var order = new List<string>();
        var session = new FakeSession { PipelineDisposed = () => order.Add("pipeline") };
        var resources = new FakeOwnedLifetime(session, () => order.Add("session"));

        var pipeline = new FakePipeline(() => order.Add("pipeline"));
        VulkanWindowRenderService.DisposeOwnedResources(null, pipeline, resources);

        Assert.Equal(2, order.Count);
        Assert.Equal("pipeline", order[0]);
        Assert.Equal("session", order[1]);
        Assert.Equal(1, session.DisposeCount);
    }

    [Fact]
    public void OwnedResourceDisposalAttemptsEveryStageAndPreservesLastFailure()
    {
        var order = new List<string>();
        var session = new FakeSession();
        var resources = new FakeOwnedLifetime(session, () => order.Add("session"));
        var textPipeline = new FakePipeline(() =>
        {
            order.Add("text");
            throw new InvalidOperationException("text dispose failure");
        });
        var graphicsPipeline = new FakePipeline(() =>
        {
            order.Add("ui");
            throw new InvalidOperationException("ui dispose failure");
        });

        var exception = Assert.Throws<InvalidOperationException>(() =>
            VulkanWindowRenderService.DisposeOwnedResources(textPipeline, graphicsPipeline, resources));

        Assert.Equal("ui dispose failure", exception.Message);
        Assert.Equal(3, order.Count);
        Assert.Equal("text", order[0]);
        Assert.Equal("ui", order[1]);
        Assert.Equal("session", order[2]);
        Assert.Equal(1, session.DisposeCount);
    }

    [Fact]
    public void FailedInitializationRollsBackPublishedResourcesAndCanRetry()
    {
        var shouldFail = true;
        var sessions = new List<FakeSession>();
        var resources = new RetriableResourceLifetime(() =>
        {
            var session = new FakeSession { ThrowOnGraphicsPipeline = shouldFail };
            sessions.Add(session);
            return session;
        });
        var platform = new Sdl3PlatformShell(new NullWindowFactory(), new WindowConfiguration("test"));
        using var service = new VulkanWindowRenderService(platform, resources, null);

        Assert.Throws<InvalidOperationException>(() => service.Initialize());
        Assert.Equal(1, resources.InitializeCount);
        Assert.Equal(1, resources.RollbackCount);
        Assert.Equal(1, sessions[0].DisposeCount);

        shouldFail = false;
        service.Initialize();
        service.Initialize();

        Assert.Equal(2, resources.InitializeCount);
        Assert.Equal(1, resources.RollbackCount);
        service.Dispose();
        Assert.Equal(1, resources.DisposeCount);
        Assert.Equal(1, sessions[1].DisposeCount);
    }

    [Fact]
    public void ResourceInitializationFailureRollsBackWithoutPublishing()
    {
        var resources = new RetriableResourceLifetime(
            () => new FakeSession(),
            throwOnInitialize: true);
        var platform = new Sdl3PlatformShell(new NullWindowFactory(), new WindowConfiguration("test"));
        using var service = new VulkanWindowRenderService(platform, resources, null);

        Assert.Throws<InvalidOperationException>(() => service.Initialize());
        Assert.Equal(1, resources.InitializeCount);
        Assert.Equal(1, resources.RollbackCount);
        service.Dispose();
        Assert.Equal(1, resources.DisposeCount);
    }

    private sealed class FakeSource : IUiRenderFrameSource, IDisposable
    {
        private readonly UiRenderBatchAdapter _adapter = new();
        private readonly UiRenderFrameToken _token;

        public FakeSource(bool withText = false)
        {
            var text = withText
                ? new[]
                {
                    new TextSubmissionRecord(
                        new TextSubmissionHandle(TextSubmissionOwnerKind.XamlElement, 1, 1),
                        TextAnchor.ScreenPixels(new TextScreenAnchor(0, 0)),
                        new TextRun(new[]
                        {
                            new TextGlyphInstance(
                                new TextAtlasPageId(1),
                                new TextUvRect(0, 0, 1, 1),
                                new TextPixelBounds(0, 0, 10, 10),
                                new TextColor(1, 1, 1, 1),
                                UiClipRect.Unbounded,
                                TextRenderMode.Sdf,
                                1,
                                0)
                        }),
                        UiClipRect.Unbounded,
                        1,
                        0)
                }
                : Array.Empty<TextSubmissionRecord>();
            _token = _adapter.Replace([new UiQuad(0, 0, 10, 10, 1, 1, 1, 1)], text, []);
        }
        public int BorrowCount { get; private set; }
        public UiRenderFrameView BorrowFrame()
        {
            BorrowCount++;
            return new UiRenderFrameView(_adapter.Borrow(in _token), []);
        }
        public void Dispose() => _adapter.Dispose();
    }

    private sealed class FakePipeline(Action? onDispose = null) : IGraphicsPipeline
    {
        private bool _disposed;

        public ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                _disposed = true;
                onDispose?.Invoke();
            }

            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeSession : IRenderWindowFrameSession
    {
        public int EndFrameCount { get; private set; }
        public int SubmitFrameCount { get; private set; }
        public int BeginFrameCount { get; private set; }
        public int CombinedEndFrameCount { get; private set; }
        public int DisposeCount { get; private set; }
        public bool TextPipelineSeen { get; private set; }
        public Action? PipelineDisposed { get; init; }
        public bool ThrowOnGraphicsPipeline { get; init; }
        public RenderWindowId WindowId => RenderWindowId.New();
        public RenderFrameState BeginFrame()
        {
            BeginFrameCount++;
            return RenderFrameState.Ready(0, new WindowMetrics(1, 1, 1));
        }
        public bool EndFrame(in RenderFrameState frameState, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, ReadOnlySpan<UiQuad> uiQuads, ReadOnlySpan<RenderRecordChange> dirtyRecords) { EndFrameCount++; return false; }
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, ReadOnlySpan<UiQuad> uiQuads, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<ITextAtlasPage> atlasPages, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords)
        {
            CombinedEndFrameCount++;
            TextPipelineSeen = textPipeline is not null;
            return TextPipelineSeen;
        }
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, ReadOnlySpan<UiQuad> uiQuads, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<TextGlyphInstance> textGlyphs, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool EndFrame(in RenderFrameState frameState, IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, in UiDrawList drawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool SubmitFrame(IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters, in UiDrawList drawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) { SubmitFrameCount++; return false; }
        public bool SubmitFrame(IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, in UiDrawList uiDrawList, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, ReadOnlySpan<ITextAtlasPage> atlasPages, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public bool SubmitFrame(IGraphicsPipeline uiPipeline, in GraphicsFrameParameters uiParameters, in UiDrawList uiDrawList, IGraphicsPipeline textPipeline, in TextFrameParameters textParameters, in TextDrawList textDrawList, ReadOnlySpan<RenderRecordChange> dirtyRecords) => false;
        public IGraphicsPipeline CreateTextPipeline(in GraphicsShaderProgram shaderProgram) => new FakePipeline(PipelineDisposed);
        public IGraphicsPipeline CreateGraphicsPipeline(in GraphicsShaderProgram shaderProgram)
        {
            if (ThrowOnGraphicsPipeline)
            {
                throw new InvalidOperationException("graphics pipeline fault");
            }

            return new FakePipeline(PipelineDisposed);
        }
        public ITextAtlasDevice CreateTextAtlasDevice() => throw new NotSupportedException();
        public bool DrawFullscreenTriangle(IGraphicsPipeline pipeline, in GraphicsFrameParameters parameters) => false;
        public bool Resize(WindowMetrics metrics) => false;
        public ValueTask DisposeAsync()
        {
            DisposeCount++;
            return ValueTask.CompletedTask;
        }
    }

    private sealed class FakeOwnedLifetime(FakeSession session, Action onDispose) : VulkanWindowRenderService.IWindowedResourceLifetime
    {
        public IRenderWindowFrameSession Session { get; } = session;
        public void Initialize(Sdl3PlatformShell platform) { }
        public void RollbackInitialization() { }
        public void Dispose()
        {
            session.DisposeAsync().AsTask().GetAwaiter().GetResult();
            onDispose();
        }
    }

    private sealed class RetriableResourceLifetime(
        Func<FakeSession> sessionFactory,
        bool throwOnInitialize = false) : VulkanWindowRenderService.IWindowedResourceLifetime
    {
        private readonly Func<FakeSession> _sessionFactory = sessionFactory;
        private readonly bool _throwOnInitialize = throwOnInitialize;
        private FakeSession? _session;

        public int InitializeCount { get; private set; }
        public int RollbackCount { get; private set; }
        public int DisposeCount { get; private set; }
        public IRenderWindowFrameSession Session => _session ?? throw new InvalidOperationException("session not initialized");

        public void Initialize(Sdl3PlatformShell platform)
        {
            InitializeCount++;
            if (_throwOnInitialize)
            {
                throw new InvalidOperationException("resource initialization fault");
            }

            _session = _sessionFactory();
        }

        public void RollbackInitialization()
        {
            RollbackCount++;
            if (_session is not null)
            {
                _session.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _session = null;
            }
        }

        public void Dispose()
        {
            DisposeCount++;
            RollbackInitialization();
        }
    }

    private sealed class NullWindowFactory : IRenderWindowFactory
    {
        public WindowCreateResult CreateWindow(WindowConfiguration configuration)
            => WindowCreateResult.Failure(new RenderDiagnosticBag());
    }
}
