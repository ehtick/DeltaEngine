using System;
using DeltaEngine.Integration;
using DeltaRender;
using DeltaRender.Platform.SDL3;
using DeltaRender.Vulkan;

namespace DeltaEngine.Windowed;

internal static class Program
{
    private static int Main(string[] args)
    {
        var frameLimit = ParseFrameLimit(args);
        var platform = new Sdl3PlatformShell(
            new Sdl3WindowFactory(),
            new WindowConfiguration("Delta Engine SDF", 960, 540, true, true));
        var renderer = new VulkanRenderer(new VulkanRendererOptions());
        using var renderService = new VulkanWindowRenderService(platform, renderer);
        using var world = new WindowedNoopWorld();
        using var ui = new WindowedNoopUi();
        using var host = new EngineHost(platform, world, renderService, ui);

        try
        {
            host.Start();
            var clock = new Sdl3FrameClock();
            while (host.IsRunning && (frameLimit is null || host.CompletedFrames < frameLimit.Value))
            {
                host.RunFrame(clock.NextDeltaSeconds());
            }

            return 0;
        }
        // The process boundary converts unexpected startup/runtime failures into a non-zero exit code.
#pragma warning disable CA1031
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
#pragma warning restore CA1031
    }

    private static int? ParseFrameLimit(string[] args)
    {
        for (var index = 0; index + 1 < args.Length; index++)
        {
            if (string.Equals(args[index], "--frames", StringComparison.OrdinalIgnoreCase) && int.TryParse(args[index + 1], out var value))
            {
                return value >= 0 ? value : throw new ArgumentOutOfRangeException(nameof(args), "--frames must be non-negative.");
            }
        }

        return null;
    }
}
