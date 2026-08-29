using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Runtime.InteropServices;
using Delta.Engine.Integration;
using Delta.Maths;
using Delta.Render;
using Delta.Render.FullscreenShaders;
using Delta.Render.Platform.SDL3;
using Delta.Render.RenderGraph;
using Delta.Render.UiShaders;
using Delta.Render.Vulkan;
using Delta.Shader.Contract;
using Delta.XAML.Contract;
using SDL3;

[assembly: SuppressMessage(
    "Design",
    "CA1515:Consider making public types internal",
    Justification = "Windowed composition types are the deliberate public opt-in boundary used by game and editor composition roots.")]

namespace Delta.Engine.Windowed;

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
        var uiEvents = new List<UiInputEvent>();
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
                    uiEvents.Add(UiInputEvent.FromKey(new UiKeyEvent(
                        UiKeyEventKind.Down,
                        new UiPhysicalKey((uint)nativeEvent.Key.Key),
                        new UiLogicalKey((uint)nativeEvent.Key.Key),
                        default,
                        nativeEvent.Key.Repeat)));
                    break;
                case SDL.EventType.KeyUp:
                    events.Add(new EngineInputEvent(EngineInputEventKind.KeyUp, Code: (int)nativeEvent.Key.Key));
                    uiEvents.Add(UiInputEvent.FromKey(new UiKeyEvent(
                        UiKeyEventKind.Up,
                        new UiPhysicalKey((uint)nativeEvent.Key.Key),
                        new UiLogicalKey((uint)nativeEvent.Key.Key),
                        default,
                        false)));
                    break;
                case SDL.EventType.MouseMotion:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerMove, X: nativeEvent.Motion.X, Y: nativeEvent.Motion.Y));
                    uiEvents.Add(UiInputEvent.FromPointingDevice(new UiPointerEvent(
                        UiPointerEventKind.Move,
                        UiPointerDeviceKind.Mouse,
                        0,
                        new float2(nativeEvent.Motion.X, nativeEvent.Motion.Y),
                        new float2(nativeEvent.Motion.XRel, nativeEvent.Motion.YRel),
                        default,
                        UiPointerButton.None,
                        default,
                        0,
                        default)));
                    break;
                case SDL.EventType.MouseButtonDown:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerDown, Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    uiEvents.Add(UiInputEvent.FromPointingDevice(new UiPointerEvent(
                        UiPointerEventKind.ButtonDown,
                        UiPointerDeviceKind.Mouse,
                        0,
                        new float2(nativeEvent.Button.X, nativeEvent.Button.Y),
                        default,
                        default,
                        new UiPointerButton((uint)nativeEvent.Button.Button),
                        default,
                        0,
                        default)));
                    break;
                case SDL.EventType.MouseButtonUp:
                    events.Add(new EngineInputEvent(EngineInputEventKind.PointerUp, Code: nativeEvent.Button.Button, X: nativeEvent.Button.X, Y: nativeEvent.Button.Y));
                    uiEvents.Add(UiInputEvent.FromPointingDevice(new UiPointerEvent(
                        UiPointerEventKind.ButtonUp,
                        UiPointerDeviceKind.Mouse,
                        0,
                        new float2(nativeEvent.Button.X, nativeEvent.Button.Y),
                        default,
                        default,
                        new UiPointerButton((uint)nativeEvent.Button.Button),
                        default,
                        0,
                        default)));
                    break;
                case SDL.EventType.MouseWheel:
                    uiEvents.Add(UiInputEvent.FromPointingDevice(new UiPointerEvent(
                        UiPointerEventKind.Wheel,
                        UiPointerDeviceKind.Mouse,
                        0,
                        new float2(nativeEvent.Wheel.MouseX, nativeEvent.Wheel.MouseY),
                        default,
                        new float2(nativeEvent.Wheel.X, nativeEvent.Wheel.Y),
                        UiPointerButton.None,
                        default,
                        0,
                        default)));
                    break;
                case SDL.EventType.TextInput:
                    var text = Marshal.PtrToStringUTF8(nativeEvent.Text.Text);
                    if (!string.IsNullOrEmpty(text))
                    {
                        uiEvents.Add(UiInputEvent.FromText(new UiTextInput(text.AsMemory())));
                    }
                    break;
            }
        }

        if (_window.IsClosed)
        {
            exitRequested = true;
        }

        return new InputSnapshot(frameNumber, exitRequested, _surface, events.ToArray(), uiEvents.ToArray());
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
        => _surface = new EngineSurfaceSnapshot(Math.Max(width, 0), Math.Max(height, 0), IsResized: true);

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

public readonly record struct WindowShaderArtifactSelection(string VertexName, string FragmentName, bool UsesUiPushConstants)
{
    public static WindowShaderArtifactSelection Fullscreen => new("fullscreen-rounded-rectangle.vert", "fullscreen-rounded-rectangle.frag", false);

    public static WindowShaderArtifactSelection UiPanel => new("ui-panel.vert", "ui-panel.frag", true);

    public static WindowShaderArtifactSelection For(bool hasUiProvider) => hasUiProvider ? UiPanel : Fullscreen;
}

internal sealed class WindowedRenderGraphFeature : IRenderFeature
{
    private readonly IGraphicsShaderProgram _fullscreenProgram;
    private readonly IGraphicsShaderProgram? _uiProgram;
    private UiQuad[] _quads = [];
    private readonly FullscreenPass _fullscreenPass = new();
    private readonly UiPass _uiPass = new();
    private int _quadCount;
    private GraphicsFrameParameters _parameters;

    public WindowedRenderGraphFeature(IGraphicsShaderProgram fullscreenProgram, IGraphicsShaderProgram? uiProgram)
    {
        _fullscreenProgram = fullscreenProgram ?? throw new ArgumentNullException(nameof(fullscreenProgram));
        _uiProgram = uiProgram;
    }

    public void Update(EngineSurfaceSnapshot surface, in UiDisplayList displayList)
    {
        _parameters = new GraphicsFrameParameters(surface.Width, surface.Height, 0);
        if (_uiProgram is null)
        {
            _quadCount = 0;
            return;
        }

        var clips = displayList.Clips;
        var order = displayList.Order;
        EnsureCapacity(order.IsEmpty ? displayList.Visuals.Length : order.Length);
        _quadCount = 0;
        if (order.IsEmpty)
        {
            for (var index = 0; index < displayList.Visuals.Length; index++)
            {
                AppendVisual(displayList.Visuals[index], clips, checked((uint)index));
            }
        }
        else
        {
            for (var index = 0; index < order.Length; index++)
            {
                var drawRef = order[index];
                if (drawRef.Kind != UiDrawKind.Visual || (uint)drawRef.Index >= (uint)displayList.Visuals.Length)
                {
                    continue;
                }

                AppendVisual(displayList.Visuals[drawRef.Index], clips, checked((uint)index));
            }
        }
    }

    public void Update(EngineSurfaceSnapshot surface)
    {
        _parameters = new GraphicsFrameParameters(surface.Width, surface.Height, 0);
        _quadCount = 0;
    }

    public void AddPasses(IRenderGraphBuilder graph, IRenderFeatureContext context)
    {
        var surface = graph.ImportSurface(context.View.Surface);
        var fullscreen = graph.AddRasterPass(
            new RasterPassDescription("engine-fullscreen", new RasterPipelineDescription(_fullscreenProgram, cullMode: RasterCullMode.None)),
            _fullscreenPass);
        graph.UseColorAttachment(
            fullscreen,
            0,
            new ColorAttachmentDescription(
                surface,
                AttachmentLoadOperation.Clear,
                AttachmentStoreOperation.Store,
                new ClearColor(0.04f, 0.05f, 0.08f, 1f)));
        _fullscreenPass.Update(_parameters);

        if (_uiProgram is null || _quadCount == 0)
        {
            return;
        }

        var ui = graph.AddRasterPass(
            new RasterPassDescription("engine-ui", new RasterPipelineDescription(_uiProgram, cullMode: RasterCullMode.None, blendMode: RenderBlendMode.Alpha)),
            _uiPass);
        graph.UseColorAttachment(
            ui,
            0,
            new ColorAttachmentDescription(surface, AttachmentLoadOperation.Load, AttachmentStoreOperation.Store));
        _uiPass.Update(_quads, _quadCount, _parameters);
    }

    private void EnsureCapacity(int required)
    {
        if (_quads.Length >= required)
        {
            return;
        }

        var capacity = Math.Max(4, _quads.Length);
        while (capacity < required)
        {
            capacity = checked(capacity * 2);
        }

        Array.Resize(ref _quads, capacity);
    }

    private void AppendVisual(UiVisualDraw visual, ReadOnlySpan<UiClipRegion> clips, uint order)
    {
        if (visual.Kind is not (UiVisualKind.SolidRectangle or UiVisualKind.RoundedRectangle or UiVisualKind.Border) ||
            !TryResolveClip(clips, visual.Clip, out var clip))
        {
            return;
        }

        var bounds = visual.Bounds;
        var color = visual.Color;
        var quad = new UiQuad(bounds.x, bounds.y, bounds.z, bounds.w, color.x, color.y, color.z, color.w)
        {
            Clip = clip,
            Order = order,
        };
        if (quad.IsValid)
        {
            _quads[_quadCount++] = quad;
        }
    }

    private static bool TryResolveClip(ReadOnlySpan<UiClipRegion> clips, UiClipId id, out UiClipRect result)
    {
        result = UiClipRect.Unbounded;
        if (!id.IsValid)
        {
            return true;
        }

        if ((uint)id.Value >= (uint)clips.Length)
        {
            return false;
        }

        result = ToClip(clips[id.Value].Bounds);
        var parent = clips[id.Value].Parent;
        var guard = clips.Length;
        while (parent.IsValid && guard-- > 0)
        {
            if ((uint)parent.Value >= (uint)clips.Length)
            {
                return false;
            }

            result = Intersect(result, ToClip(clips[parent.Value].Bounds));
            parent = clips[parent.Value].Parent;
        }

        return result.IsValid;
    }

    private static UiClipRect ToClip(float4 bounds) => new(bounds.x, bounds.y, bounds.z, bounds.w);

    private static UiClipRect Intersect(UiClipRect left, UiClipRect right)
    {
        if (left.IsUnbounded)
        {
            return right;
        }

        if (right.IsUnbounded)
        {
            return left;
        }

        var x = MathF.Max(left.X, right.X);
        var y = MathF.Max(left.Y, right.Y);
        var r = MathF.Min(left.X + left.Width, right.X + right.Width);
        var b = MathF.Min(left.Y + left.Height, right.Y + right.Height);
        return new UiClipRect(x, y, MathF.Max(0, r - x), MathF.Max(0, b - y));
    }

    private sealed class FullscreenPass : IRasterPass
    {
        private GraphicsFrameParameters _parameters;

        public void Update(GraphicsFrameParameters parameters) => _parameters = parameters;

        public void Record(IRasterCommandContext commands)
        {
            var viewport = new RenderViewport(0, 0, _parameters.ResolutionX, _parameters.ResolutionY);
            var scissor = new PixelRect(0, 0, checked((int)_parameters.ResolutionX), checked((int)_parameters.ResolutionY));
            commands.SetViewport(in viewport);
            commands.SetScissor(in scissor);
            var constants = new FullscreenUi.UiPushConstants
            {
                Resolution = new float2(_parameters.ResolutionX, _parameters.ResolutionY),
                Time = _parameters.TimeSeconds,
            };
            commands.PushConstants(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref constants, 1)));
            commands.Draw(3);
        }
    }

    private sealed class UiPass : IRasterPass
    {
        private UiQuad[] _quads = [];
        private int _quadCount;
        private GraphicsFrameParameters _parameters;

        public void Update(UiQuad[] quads, int quadCount, GraphicsFrameParameters parameters)
        {
            _quads = quads;
            _quadCount = quadCount;
            _parameters = parameters;
        }

        public void Record(IRasterCommandContext commands)
        {
            var viewport = new RenderViewport(0, 0, _parameters.ResolutionX, _parameters.ResolutionY);
            commands.SetViewport(in viewport);
            var metrics = new WindowMetrics((uint)_parameters.ResolutionX, (uint)_parameters.ResolutionY, 1);
            var constants = new UiPanel.Parameters
            {
                Resolution = new float2(_parameters.ResolutionX, _parameters.ResolutionY),
            };
            for (var index = 0; index < _quadCount; index++)
            {
                var quad = _quads[index];
                if (!quad.Clip.TryGetScissor(metrics, out var clip))
                {
                    continue;
                }

                var scissor = new PixelRect(clip.X, clip.Y, checked((int)clip.Width), checked((int)clip.Height));
                commands.SetScissor(in scissor);
                constants.Rect = new float4(quad.X, quad.Y, quad.Width, quad.Height);
                constants.Color = new float4(quad.Red, quad.Green, quad.Blue, quad.Alpha);
                commands.PushConstants(MemoryMarshal.AsBytes(MemoryMarshal.CreateReadOnlySpan(ref constants, 1)));
                commands.Draw(6);
            }
        }
    }
}

public sealed class VulkanWindowRenderService : IEngineRenderService
{
    internal interface IWindowedResourceLifetime : IDisposable
    {
        IRenderFrameSession Session { get; }
        void Initialize(Sdl3PlatformShell platform);
        void RollbackInitialization();
    }

    private sealed class BorrowedResourceLifetime(IRenderFrameSession session) : IWindowedResourceLifetime
    {
        public IRenderFrameSession Session { get; } = session ?? throw new ArgumentNullException(nameof(session));
        public void Initialize(Sdl3PlatformShell platform) { }
        public void RollbackInitialization() { }
        public void Dispose() { }
    }

    private sealed class OwnedResourceLifetime(VulkanRenderer renderer) : IWindowedResourceLifetime
    {
        private readonly VulkanRenderer _renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        private IRenderFrameSession? _session;

        public IRenderFrameSession Session => _session ?? throw new InvalidOperationException("Window resources are not initialized.");

        public void Initialize(Sdl3PlatformShell platform)
        {
            if (_session is null)
            {
                _session = _renderer.CreateWindowSession(platform.Window);
            }
        }

        public void RollbackInitialization()
        {
            if (_session is not null)
            {
                _session.DisposeAsync().AsTask().GetAwaiter().GetResult();
                _session = null;
            }
        }

        public void Dispose()
        {
            try
            {
                _session?.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
            finally
            {
                _session = null;
                _renderer.DisposeAsync().AsTask().GetAwaiter().GetResult();
            }
        }
    }

    private readonly Sdl3PlatformShell _platform;
    private readonly IWindowedResourceLifetime _resources;
    private readonly IEngineUiDisplayListSource? _uiSource;
    private IRenderFrameSession? _session;
    private IRenderGraph? _graph;
    private IGraphicsPipeline? _graphicsPipeline;
    private IGraphicsPipeline? _uiPipeline;
    private WindowedRenderGraphFeature? _feature;
    private IRenderFeature[] _features = [];
    private RenderView[] _views = [];
    private EngineSurfaceSnapshot _lastSurface;
    private bool _initialized;
    private bool _disposed;

    public VulkanWindowRenderService(
        Sdl3PlatformShell platform,
        VulkanRenderer renderer,
        IEngineUiDisplayListSource? uiSource = null)
        : this(platform, new OwnedResourceLifetime(renderer), uiSource)
    {
    }

    public VulkanWindowRenderService(
        Sdl3PlatformShell platform,
        IRenderFrameSession session,
        IEngineUiDisplayListSource? uiSource = null)
        : this(platform, new BorrowedResourceLifetime(session), uiSource)
    {
    }

    internal VulkanWindowRenderService(
        Sdl3PlatformShell platform,
        IWindowedResourceLifetime resources,
        IEngineUiDisplayListSource? uiSource)
    {
        _platform = platform ?? throw new ArgumentNullException(nameof(platform));
        _resources = resources ?? throw new ArgumentNullException(nameof(resources));
        _uiSource = uiSource;
    }

    public void Initialize()
    {
        ThrowIfDisposed();
        if (_initialized)
        {
            return;
        }

        IRenderFrameSession? session = null;
        IRenderGraph? graph = null;
        IGraphicsPipeline? graphicsPipeline = null;
        IGraphicsPipeline? uiPipeline = null;
        try
        {
            _resources.Initialize(_platform);
            session = _resources.Session;
            var fullscreenSelection = WindowShaderArtifactSelection.Fullscreen;
            var fullscreenProgram = FullscreenUiGraphicsShaderProgram.CreateProgram(
                LoadSpirv(fullscreenSelection.VertexName),
                LoadSpirv(fullscreenSelection.FragmentName));
            graphicsPipeline = session.CreateGraphicsPipeline(in fullscreenProgram);

            IGraphicsShaderProgram? uiProgram = null;
            if (_uiSource is not null)
            {
                var uiSelection = WindowShaderArtifactSelection.UiPanel;
                uiProgram = UiPanelGraphicsShaderProgram.CreateProgram(
                    LoadSpirv(uiSelection.VertexName),
                    LoadSpirv(uiSelection.FragmentName));
                uiPipeline = session.CreateGraphicsPipeline(in uiProgram);
            }

            graph = session.CreateRenderGraph();
            var feature = new WindowedRenderGraphFeature(fullscreenProgram, uiProgram);
            var features = new IRenderFeature[] { feature };
            var views = new RenderView[1];
            var lastSurface = _platform.Surface;
            _session = session;
            _graph = graph;
            _graphicsPipeline = graphicsPipeline;
            _uiPipeline = uiPipeline;
            _feature = feature;
            _features = features;
            _views = views;
            _lastSurface = lastSurface;
            _initialized = true;
        }
        catch
        {
            DisposeInitializationFailure(graph as IAsyncDisposable, uiPipeline, graphicsPipeline, _resources);
            throw;
        }
    }

    public void Render(in EngineRenderFrame frame)
    {
        ThrowIfDisposed();
        if (!_initialized || _session is null || _graph is null || _feature is null)
        {
            throw new InvalidOperationException("Window render service must be initialized before rendering.");
        }

        if (!frame.Surface.IsValid)
        {
            return;
        }

        if (_lastSurface != frame.Surface)
        {
            if (!_session.Resize(new WindowMetrics((uint)frame.Surface.Width, (uint)frame.Surface.Height, 1f)))
            {
                throw new InvalidOperationException("Vulkan swapchain resize failed.");
            }

            _lastSurface = frame.Surface;
        }

        if (_uiSource is not null)
        {
            var displayList = _uiSource.BorrowDisplayList();
            _feature.Update(frame.Surface, in displayList);
        }
        else
        {
            _feature.Update(frame.Surface);
        }

        _views[0] = new RenderView(
            _session.SurfaceHandle,
            new RenderViewport(0, 0, frame.Surface.Width, frame.Surface.Height),
            new PixelRect(0, 0, frame.Surface.Width, frame.Surface.Height));
        var graphFrame = new RenderGraphFrame(frame.FrameNumber, _views);
        _graph.Build(in graphFrame, _features);
        _graph.Execute();
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
        var graph = _graph;
        var uiPipeline = _uiPipeline;
        var graphicsPipeline = _graphicsPipeline;
        _graph = null;
        _uiPipeline = null;
        _graphicsPipeline = null;
        _feature = null;
        _features = [];
        _views = [];
        _session = null;
        _initialized = false;
        DisposeOwnedResources(graph as IAsyncDisposable, uiPipeline, graphicsPipeline, _resources);
    }

    internal static void DisposeOwnedResources(
        IAsyncDisposable? graph,
        IGraphicsPipeline? uiPipeline,
        IGraphicsPipeline? graphicsPipeline,
        IWindowedResourceLifetime resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        try
        {
            DisposeAsync(graph);
        }
        finally
        {
            try
            {
                DisposeAsync(uiPipeline);
            }
            finally
            {
                try
                {
                    DisposeAsync(graphicsPipeline);
                }
                finally
                {
                    resources.Dispose();
                }
            }
        }
    }

    private static void DisposeInitializationFailure(
        IAsyncDisposable? graph,
        IGraphicsPipeline? uiPipeline,
        IGraphicsPipeline? graphicsPipeline,
        IWindowedResourceLifetime resources)
    {
        try
        {
            DisposeAsync(graph);
        }
        finally
        {
            try
            {
                DisposeAsync(uiPipeline);
            }
            finally
            {
                try
                {
                    DisposeAsync(graphicsPipeline);
                }
                finally
                {
                    resources.RollbackInitialization();
                }
            }
        }
    }

    private static void DisposeAsync(IAsyncDisposable? resource)
        => resource?.DisposeAsync().AsTask().GetAwaiter().GetResult();

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private static byte[] LoadSpirv(string name)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "shaders", name + ".spv");
        try
        {
            return File.ReadAllBytes(path);
        }
        catch (FileNotFoundException exception)
        {
            throw new InvalidOperationException($"Missing shader artifact '{path}'. Build the DeltaRender smoke shader assets before running the windowed composition.", exception);
        }
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
    public static SdfFrameUniforms CreateUniforms(EngineSurfaceSnapshot surface)
        => new(new float2(surface.Width, surface.Height), 0);
}
