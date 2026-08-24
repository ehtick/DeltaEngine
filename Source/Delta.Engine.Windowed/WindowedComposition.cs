using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Diagnostics.CodeAnalysis;
using Delta.Engine.Integration;
using Delta.Maths;
using Delta.Render.Core;
using Delta.Render.Platform.SDL3;
using Delta.Render.Vulkan;
using Delta.Shader.Abstractions;
using SDL3;

[assembly: SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "Windowed composition types are the deliberate public opt-in boundary used by game and editor composition roots.")]

namespace Delta.Engine.Windowed;

#nullable enable

public sealed class Sdl3PlatformShell : IEnginePlatformShell
{
    private readonly IRenderWindowFactory _windowFactory;
    private readonly WindowConfiguration _configuration;
    private IRenderWindow? _window;
    private bool _disposed;
    private EngineSurfaceSnapshot _surface;

    public Sdl3PlatformShell(IRenderWindowFactory windowFactory, WindowConfiguration configuration)
    {
        _windowFactory = windowFactory ?? throw new ArgumentNullException(nameof(windowFactory));
        _configuration = configuration;
    }

    public IRenderWindow Window => _window ?? throw new InvalidOperationException("SDL3 platform shell is not initialized.");

    public EngineSurfaceSnapshot Surface => _surface;

    public void Initialize()
    {
        ThrowIfDisposed();
        if (_window is not null)
        {
            return;
        }

        var result = _windowFactory.CreateWindow(_configuration);
        if (!result.Success || result.Window is null)
        {
            throw new InvalidOperationException($"SDL3 window creation failed: {result.Diagnostics.ToText()}");
        }

        _window = result.Window;
        UpdateSurface((int)_configuration.Width, (int)_configuration.Height);
    }

    public InputSnapshot PollInput(int frameNumber, float deltaSeconds)
    {
        ThrowIfDisposed();
        if (_window is null)
        {
            throw new InvalidOperationException("SDL3 platform shell must be initialized before polling input.");
        }

        var events = new List<EngineInputEvent>();
        var uiPackets = new List<EngineUiInputPacket>();
        var exitRequested = _window.IsClosed;
        while (SDL.PollEvent(out var nativeEvent))
        {
            switch ((SDL.EventType)nativeEvent.Type)
            {
                case SDL.EventType.Quit:
                case SDL.EventType.WindowCloseRequested:
                    exitRequested = true;
                    events.Add(new EngineInputEvent(EngineInputEventKind.Quit));
                    break;
                case SDL.EventType.WindowResized:
                case SDL.EventType.WindowPixelSizeChanged:
                    UpdateSurface(nativeEvent.Window.Data1, nativeEvent.Window.Data2);
                    break;
                case SDL.EventType.KeyDown:
                    events.Add(new EngineInputEvent(EngineInputEventKind.KeyDown, Code: (int)nativeEvent.Key.Key));
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.KeyDown,
                        Code: (int)nativeEvent.Key.Key, IsRepeat: nativeEvent.Key.Repeat));
                    break;
                case SDL.EventType.KeyUp:
                    events.Add(new EngineInputEvent(EngineInputEventKind.KeyUp, Code: (int)nativeEvent.Key.Key));
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.KeyUp,
                        Code: (int)nativeEvent.Key.Key));
                    break;
                case SDL.EventType.MouseMotion:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerMove, X: nativeEvent.Motion.X, Y: nativeEvent.Motion.Y));
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.PointerMove,
                        X: nativeEvent.Motion.X, Y: nativeEvent.Motion.Y,
                        DeltaX: nativeEvent.Motion.XRel, DeltaY: nativeEvent.Motion.YRel));
                    break;
                case SDL.EventType.MouseButtonDown:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerDown, Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.PointerDown,
                        Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    break;
                case SDL.EventType.MouseButtonUp:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerUp, Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.PointerUp,
                        Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    break;
                case SDL.EventType.MouseWheel:
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.Wheel,
                        X: nativeEvent.Wheel.MouseX, Y: nativeEvent.Wheel.MouseY,
                        DeltaX: nativeEvent.Wheel.X, DeltaY: nativeEvent.Wheel.Y));
                    break;
                case SDL.EventType.TextInput:
                    uiPackets.Add(new EngineUiInputPacket(EngineUiInputKind.TextInput,
                        Text: Marshal.PtrToStringUTF8(nativeEvent.Text.Text)));
                    break;
            }
        }

        if (_window.IsClosed)
        {
            exitRequested = true;
        }

        return new InputSnapshot(frameNumber, exitRequested, _surface, events.ToArray(), uiPackets.ToArray());
    }

    public void Shutdown()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_window is not null)
        {
            _window.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _window = null;
        }

        SDL.Quit();
    }

    private void UpdateSurface(int width, int height)
    {
        _surface = new EngineSurfaceSnapshot(Math.Max(width, 0), Math.Max(height, 0), IsResized: true);
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);
}

public sealed class Sdl3FrameClock : IEngineFrameClock
{
    private long _lastTicks;

    public float NextDeltaSeconds()
    {
        var ticks = (long)SDL.GetTicksNS();
        if (_lastTicks == 0)
        {
            _lastTicks = ticks;
            return 0;
        }

        var delta = Math.Max(0, ticks - _lastTicks) / 1_000_000_000f;
        _lastTicks = ticks;
        return Math.Min(delta, 0.25f);
    }
}

public readonly record struct WindowShaderArtifactSelection(
    string VertexName,
    string FragmentName,
    bool UsesUiPushConstants)
{
    public static WindowShaderArtifactSelection Fullscreen => new(
        "fullscreen-rounded-rectangle.vert",
        "fullscreen-rounded-rectangle.frag",
        false);

    public static WindowShaderArtifactSelection UiPanel => new(
        "ui-panel.vert",
        "ui-panel.frag",
        true);

    public static WindowShaderArtifactSelection For(bool hasUiProvider)
        => hasUiProvider ? UiPanel : Fullscreen;
}

internal static class WindowedUiFrameSubmission
{
    public static void EndFrameOrThrow(
        IRenderWindowFrameSession session,
        IUiRenderFrameSource source,
        in RenderFrameState frameState,
        IGraphicsPipeline pipeline,
        in GraphicsFrameParameters parameters,
        ref TextGlyphInstance[] orderedGlyphs,
        ref TextBatchRange[] textBatches)
    {
        var frameView = source.BorrowFrame();
        var batch = frameView.Batch;
        EnsureTextScratch(batch.TextSubmissions, ref orderedGlyphs, ref textBatches);
        var textParameters = new TextFrameParameters(
            parameters.ResolutionX,
            parameters.ResolutionY,
            parameters.TimeSeconds,
            new TextColor(1, 1, 1, 1),
            new TextColor(0, 0, 0, 0),
            0);
        var projectionContext = new TextProjectionContext(
            frameState.Metrics.Width,
            frameState.Metrics.Height,
            frameState.Metrics.DpiScale);
        var frameSucceeded = session.EndPreparedFrame(
            in frameState,
            pipeline,
            in parameters,
            textPipeline: null,
            in textParameters,
            in frameView,
            in projectionContext,
            worldProjection: null,
            orderedGlyphs,
            textBatches);
        if (!frameSucceeded)
        {
            throw new InvalidOperationException("Vulkan UI frame submission failed.");
        }
    }

    internal static void EnsureTextScratch(
        ReadOnlySpan<TextSubmissionRecord> submissions,
        ref TextGlyphInstance[] orderedGlyphs,
        ref TextBatchRange[] textBatches)
    {
        var required = 0;
        for (var index = 0; index < submissions.Length; index++)
        {
            required = checked(required + submissions[index].Glyphs.Glyphs.Length);
        }

        orderedGlyphs = EnsureCapacity(orderedGlyphs, required);
        textBatches = EnsureCapacity(textBatches, required);
    }

    private static T[] EnsureCapacity<T>(T[] storage, int required)
    {
        if (storage.Length >= required)
        {
            return storage;
        }

        var capacity = Math.Max(4, storage.Length);
        while (capacity < required)
        {
            capacity = checked(capacity * 2);
        }

        return new T[capacity];
    }
}

public sealed class VulkanWindowRenderService : IEngineRenderService
{
    private readonly Sdl3PlatformShell _platform;
    private readonly VulkanRenderer _renderer;
    private readonly IUiRenderFrameSource? _uiRenderFrameSource;
    private IRenderWindowFrameSession? _session;
    private IGraphicsPipeline? _graphicsPipeline;
    private TextGlyphInstance[] _textGlyphScratch = [];
    private TextBatchRange[] _textBatchScratch = [];
    private EngineSurfaceSnapshot _lastSurface;
    private bool _disposed;

    public VulkanWindowRenderService(
        Sdl3PlatformShell platform,
        VulkanRenderer renderer,
        IUiRenderFrameSource? uiRenderFrameSource = null)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        _uiRenderFrameSource = uiRenderFrameSource;
    }

    public void Initialize()
    {
        ThrowIfDisposed();
        _session = _renderer.CreateWindowSession(_platform.Window);
        var selection = WindowShaderArtifactSelection.For(_uiRenderFrameSource is not null);
        var vertex = LoadShaderArtifact(selection.VertexName);
        var fragment = LoadShaderArtifact(selection.FragmentName);
        var program = new GraphicsShaderProgram(vertex, fragment);
        _graphicsPipeline = _session.CreateGraphicsPipeline(in program);
        _lastSurface = _platform.Surface;
    }

    public void Render(in EngineFrameContext context)
    {
        ThrowIfDisposed();
        if (_session is null)
        {
            throw new InvalidOperationException("Window render service must be initialized before rendering.");
        }

        if (context.Surface.IsValid)
        {
            if (_lastSurface != context.Surface)
            {
                if (!_session.Resize(new WindowMetrics((uint)context.Surface.Width, (uint)context.Surface.Height, 1.0f)))
                {
                    throw new InvalidOperationException("Vulkan swapchain resize failed.");
                }

                _lastSurface = context.Surface;
            }
        }

        var frameState = _session.BeginFrame();
        if (!frameState.IsValid)
        {
            return;
        }

        if (_graphicsPipeline is null)
        {
            throw new InvalidOperationException("Fullscreen graphics pipeline is not initialized.");
        }

        var uniforms = FullscreenSdfShaderFixture.CreateUniforms(context.Surface, context.ElapsedSeconds);
        var parameters = new GraphicsFrameParameters(uniforms.Resolution.x, uniforms.Resolution.y, uniforms.TimeSeconds);

        if (_uiRenderFrameSource is not null)
        {
            WindowedUiFrameSubmission.EndFrameOrThrow(
                _session,
                _uiRenderFrameSource,
                in frameState,
                _graphicsPipeline,
                in parameters,
                ref _textGlyphScratch,
                ref _textBatchScratch);
            return;
        }

        var drawSucceeded = _session.DrawFullscreenTriangle(_graphicsPipeline, in parameters);
        var frameSucceeded = _session.EndFrame(in frameState, ReadOnlySpan<RenderRecordChange>.Empty);
        if (!drawSucceeded || !frameSucceeded)
        {
            throw new InvalidOperationException("Vulkan window frame submission failed.");
        }
    }

    public void Shutdown()
    {
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        if (_graphicsPipeline is not null)
        {
            _graphicsPipeline.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _graphicsPipeline = null;
        }
        if (_session is not null)
        {
            _session.DisposeAsync().AsTask().GetAwaiter().GetResult();
            _session = null;
        }

        _textGlyphScratch = [];
        _textBatchScratch = [];

        _renderer.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private static ShaderArtifact LoadShaderArtifact(string name)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "shaders");
        var spirvPath = Path.Combine(directory, name + ".spv");
        var manifestPath = Path.Combine(directory, name + ".shader.json");
        var manifest = JsonSerializer.Deserialize<Delta.Shader.Abstractions.ShaderAbiManifest>(File.ReadAllText(manifestPath))
            ?? throw new InvalidDataException($"Shader manifest was empty: {manifestPath}");
        return new ShaderArtifact(File.ReadAllBytes(spirvPath), manifest);
    }
}

public sealed class WindowedNoopWorld : IEngineWorldService
{
    public void Initialize() { }
    public void Update(in EngineFrameContext context) { }
    public void Shutdown() { }
    public void Dispose() { }
}

public sealed class WindowedNoopUi : IEngineUiService
{
    public void Initialize() { }
    public void Update(in EngineFrameContext context) { }
    public void Shutdown() { }
    public void Dispose() { }
}

public readonly record struct SdfFrameUniforms(float2 Resolution, float TimeSeconds);

public static class FullscreenSdfShaderFixture
{
    public static SdfFrameUniforms CreateUniforms(EngineSurfaceSnapshot surface, float elapsedSeconds)
        => new(new float2(surface.Width, surface.Height), elapsedSeconds);
}
