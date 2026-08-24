# Windowed composition slice

`Delta.Engine.Windowed` is an optional composition project. It is the owner of
the SDL3 event loop, input translation, close handling, resize observation and
frame clock. `EngineHost` still owns deterministic stage order:

```text
SDL3 poll -> InputSnapshot -> world/user update
  -> UI update/layout -> render submission -> present
```

This is the target same-frame order. The historical `EngineHost` contract
still records `RenderUpdated` before `UiUpdated`; phase 2 of the repair plan
migrates implementation and stage-order tests together.

The renderer receives surface metrics and frame time through
`EngineFrameContext`; it never polls SDL input. The headless `Delta.Engine`
boundary remains usable with `NullRenderer` and has no Render, Vulkan, SDL3 or
Shader project reference.

The fullscreen sample is a historical lower-level smoke, not the current
editor UI composition path. It uses the real `Delta.Render.Core` window/session contracts and
`Delta.Render.Vulkan` swapchain lifecycle. It loads the versioned vertex and
fragment `ShaderArtifact` outputs of DeltaShader, creates a Vulkan graphics
pipeline, and draws the fullscreen SDF rectangle every frame. Resolution is
carried through Delta.Maths `float2`; elapsed time comes from `EngineHost`.

| Dependency | Current usable contract | Windowed slice status |
| --- | --- | --- |
| Delta.Maths | `float2` and related runtime value types | Used by SDF uniform description |
| DeltaShader | C# vertex/fragment shaders and versioned graphics ABI | Generated fullscreen artifacts are consumed |
| Delta.Render | SDL3 window, Vulkan surface, swapchain and graphics pipeline | Fullscreen draw is connected |
| SDL3-CS | Native event polling/window operations | Owned by the platform shell |
| MoltenVK | Render sibling loads it on macOS | Available only in a real macOS display smoke |
| DeltaECS | Not required by this composition slice | World remains a neutral host service |

## Run

Build and test the optional composition project:

```bash
dotnet test Source/Delta.Engine.Windowed.Tests/Delta.Engine.Windowed.Tests.csproj -c Release
```

On macOS with a display, SDL3 and MoltenVK available, run one bounded frame:

```bash
dotnet run --project Source/Delta.Engine.Windowed/Delta.Engine.Windowed.csproj -c Release -- --frames 1
```

## Current UI layer

DeltaXAML already parses and retains the UI tree and emits the renderer-neutral
`IUiDrawList`. Production migration uses
`IUiDrawList -> UiRenderBatchAdapter -> borrowed UiRenderBatch`; Engine only
schedules the frame and translates SDL input. `EngineUiQuad` is a temporary
migration adapter and must not become a second UI model.

The next step is consumer convergence and real DeltaText glyph submission, not
another draw-list contract. See
[../../HIGH_PRIORITY_TODO.md](../../HIGH_PRIORITY_TODO.md) and
[../../EDITOR_UI_TODO.md](../../EDITOR_UI_TODO.md).
