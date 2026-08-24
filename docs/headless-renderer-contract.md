# Headless renderer boundary

The legacy Vulkan/Silk renderer implementation has been removed from the
engine-owned core. `Delta.Engine.Integration` currently contains two
migration-era layers that must not be treated as competing public lifecycles:

- `IEngineRenderService` + `EngineFrameContext` is the host-facing
  lifecycle/frame contract.
- `IRenderFrameSink`/`IRenderer` is the lower headless/backend adapter and
  is scheduled for internalization or removal after consumer migration.
- `NullRenderer` reports no backend and performs no GPU/window work.
- `NullGraphicsModule` drives deterministic resize and frame calls for
  headless engine/editor/game runs.

The ordered consolidation is tracked in
[../../HIGH_PRIORITY_TODO.md](../../HIGH_PRIORITY_TODO.md). Source contracts,
not duplicated snippets in this document, are authoritative.

The core `Delta.Engine` project no longer references Vulkan, Silk.NET Vulkan,
Silk.NET Windowing, Delta.Shader, or `Delta.Render`. Runtime context creation uses
`NullGraphicsModule` for both headless and the currently-unimplemented
windowed path. SDL3-CS platform ownership remains outside this legacy core
layer and is not replaced here.

The optional `Source/Delta.Engine.ComputeSmoke` project is intentionally not in
the core solution. It references public `Delta.Render.Core` and
`Delta.Render.Vulkan` contracts only and can be built/run when the sibling
`DeltaRender` repository is available. Its absence must not affect core or
headless integration tests.

The removed renderer code included the old Vulkan device/swapchain/frame
implementation, render batchers, GPU collections, shader/pipeline helpers,
windowed/headless graphics modules, and `RenderStream` coupling. Scene/assets
and world-change contracts remain engine-owned data/API; backend adapters can
consume them later without reintroducing a core renderer dependency.
