# First engine delivery contracts and migration inventory

Date: 2026-08-18
Scope: `Delta.Engine` workstream only.

> Historical delivery snapshot. It explains why migration adapters exist but
> is not the current ABI source of truth. Read
> `Source/Delta.Engine.Integration/EngineIntegrationContracts.cs` and
> `EngineRendererContracts.cs` for current signatures, and
> [../../HIGH_PRIORITY_TODO.md](../../HIGH_PRIORITY_TODO.md) for their ordered
> consolidation. The snippets and dependency inventory below are intentionally
> not maintained as a second public contract.

## Namespace convention

All owned projects in `Source` use the `Delta.Engine` root. The namespace migration
also renames owned project paths, assembly names, and package identities to the same
`Delta.Engine.*` family:

| Owned identifier |
| --- |
| `Delta.Engine` |
| `Delta.Engine.Editor` |
| `Delta.Engine.EditorLib` |
| `Delta.Engine.Generation` |
| `Delta.Engine.Generation.Core` |
| `Delta.Engine.Generation.Internal` |
| `Delta.Engine.Generation.Tests` |
| `Delta.Engine.Benchmarks` |
| `Delta.Engine.Runner` |
| `Delta.Engine.ConsoleModelImporter` |
| `Delta.Engine.Integration` |
| `Delta.Engine.Integration.Tests` |

`Depend/Arch` and its namespaces are external/vendored and are intentionally
unchanged. This is a source, assembly, and package identity compatibility break for
consumers that reference pre-migration owned projects; the rename is intentionally
explicit so project, namespace, and artifact identities agree.

## Dependency/migration inventory snapshot

| Layer | Current Delta.Engine dependency | Status | Replacement path |
| --- | --- | --- | --- |
| Runtime storage | `Depend/Arch` project reference in `Source/Delta.Engine/Delta.Engine.csproj` | Kept for migration compatibility | `DeltaECS` must expose contracts used by engine boundaries |
| Vulkan/graphics wrappers | `Silk.NET.Vulkan`, `Silk.NET.Vulkan.Extensions.*` and `Silk.NET.Windowing` in `Source/Delta.Engine/Delta.Engine.csproj` | Kept for compatibility in legacy renderer/runtime | `DeltaRender` should provide SDL3 + Vulkan/MoltenVK implementation through renderer-facing contracts |
| Renderer windowed stack | `Delta.Engine/Rendering` classes (`Windowed*`, `Headless*`) | Present and compile-only in this pass | Replace by `DeltaRender` adapter + sample surface/present |
| Editor composition | `Avalonia` packages in `Source/Delta.Engine.Editor/Delta.Engine.Editor.csproj` | Kept while migration is incremental | `Delta`-owned XAML runtime and editor UI should replace direct Avalonia usage later |
| Math/shader tooling | `Delta.Maths` and DeltaShader are external references outside this repo | `Delta.Maths` project/API available | Runtime math now consumes `Delta.Maths`; DeltaShader remains an adapter boundary |

## Engine-facing contracts added in this pass

Source: `Source/Delta.Engine.Integration/EngineIntegrationContracts.cs`

```csharp
public readonly record struct InputSnapshot(
    int FrameNumber,
    bool ExitRequested = false);

public readonly record struct EngineFrameContext(
    int FrameNumber,
    float DeltaSeconds,
    InputSnapshot Input);

public interface IEngineHost : IDisposable
{
    bool IsRunning { get; }
    bool IsDisposed { get; }
    int CompletedFrames { get; }
    IReadOnlyList<EngineLifecycleEvent> StageLog { get; }
    void Start();
    void RunFrame(float deltaSeconds);
    void Shutdown();
}

public interface IEngineInputService : IDisposable
{
    void Initialize();
    InputSnapshot PollInput(int frameNumber, float deltaSeconds);
    void Shutdown();
}

public interface IEngineWorldService : IDisposable
{
    void Initialize();
    void Update(in EngineFrameContext context);
    void Shutdown();
}

public interface IEngineRenderService : IDisposable
{
    void Initialize();
    void Render(in EngineFrameContext context);
    void Shutdown();
}

public interface IEngineUiService : IDisposable
{
    void Initialize();
    void Update(in EngineFrameContext context);
    void Shutdown();
}
```

No SDL/Vulkan/Avalonia/Arch types are allowed in these signatures.

`IEngineRenderService.Render(EngineFrameContext)` is a temporary adapter boundary
for this first delivery. The renderer must not poll, own, or otherwise coordinate
input; input is polled by `IEngineInputService` and passed through the host. The
permanent renderer boundary will be an extracted `RenderPacket` contract once the
renderer and ECS owners provide those APIs.

## Deterministic lifecycle stages enforced by `EngineHost`

Implemented in `Source/Delta.Engine.Integration/EngineHost.cs`:

1. `InputInitialized`
2. `WorldInitialized`
3. `RenderInitialized`
4. `UiInitialized`
5. For each frame:
   1. `FrameStarted`
   2. `InputPolled`
   3. `WorldUpdated`
   4. `RenderUpdated`
   5. `UiUpdated`
   6. `FrameCompleted`
6. `ShutdownStarted`
7. `InputShutdown`
8. `WorldShutdown`
9. `RenderShutdown`
10. `UiShutdown`
11. `HostDisposalStarted`
12. `UiDisposed`
13. `RenderDisposed`
14. `WorldDisposed`
15. `InputDisposed`
16. `HostDisposed`

This stage order is tested via contract tests.

`Start`, `Shutdown`, and `Dispose` are idempotent. A frame is counted only after
`FrameCompleted`; exceptions from services propagate to the caller and do not
produce a completed frame.

## Contracts requested from ECS owner

`DeltaECS` exposes the time-free `Delta.ECS.Integration.IEcsWorld` lifecycle:

- `void Initialize()`
- `void Update()`
- `void Shutdown()`
- `void Dispose()`

`EcsWorldService` adapts that world to the Engine-owned `IEngineWorldService`.
DeltaEngine schedules systems and selects their fixed-step, scaled, unscaled
or editor-time inputs explicitly; it does not forward a clock into
`IEcsWorld.Update`. The parameterless ECS `Update` remains a lifecycle safe
point.

## Contracts requested from renderer owner

`DeltaRender` must provide an adapter that can satisfy `IEngineRenderService`:

- `void Initialize()`
- `void Render(in EngineRenderFrame frame)`
- `void Shutdown()`
- `void Dispose()`

`EngineRenderFrame` carries only frame identity and surface state. It does not
carry delta/elapsed time or input. Animated render features receive the chosen
Engine time domain as explicit extracted shader/feature data before submission.

## Coexistence adapters

To keep current and new architecture compiling together, temporary adapters were added:

- `Source/Delta.Engine/Integration/LegacyAdapters.cs` (kept in the legacy project because its implementation types depend on the old runtime)
- `Source/Delta.Engine/Delta.Engine.csproj` references `Source/Delta.Engine.Integration/Delta.Engine.Integration.csproj` so those adapters consume the shared contracts without duplicating them.
- `SceneWorldAdapter` for `Delta.Runtime.Scene`
- `LegacyRenderAdapter` for existing graphics module API
- `LegacyInputAdapter`
- `LegacyUiAdapter`

These adapters are intentionally minimal and are only used as migration bridges until
`DeltaECS` and `DeltaRender` own the equivalent implementations.

## Runner/glue skeleton

`Source/Delta.Engine.Integration` now also owns the platform-neutral runner contracts:

- `IEnginePlatformShell` is the SDL3-CS adapter boundary. It owns window/events/input
  and reports `EngineSurfaceSnapshot`; SDL types must not cross this boundary.
- `EngineFrameLoop` owns only the deterministic clock-driven loop and delegates lifecycle
  and frame execution to `IEngineHost`.
- `EngineFrameContext.Surface` is observed by the host and projected into the
  time-free `EngineRenderFrame`. The renderer never polls or owns input or a
  clock.
- `EngineRenderServiceAdapter` forwards the first valid surface change to an
  `IEngineRenderFrameSink` before forwarding the frame. This is still an adapter boundary;
  it is not the permanent `RenderPacket` API.
- `IEngineShaderModuleSource` is the shader-owner boundary. It exchanges opaque module bytes
  by `EngineShaderId`; DeltaShader reflection/compiler types stay inside the shader adapter.

The dependency direction is:

`Delta.Maths` -> `Delta.Engine` runtime data; `Delta.Engine.Integration` contains only neutral
contracts; future `Delta.ECS`, `Delta.Render`, and DeltaShader adapters may depend on the
integration contracts, but the integration project must not depend on any of them. The
legacy `Delta.Engine` project may reference the integration project for coexistence adapters;
the new sibling projects must not reference the legacy project.

## ECS dirty ownership contract

`IEngineWorldAccess` deliberately separates access modes:

- `TryRead` and `ReadAll` expose readonly snapshots/spans and must not mark data dirty.
- `GetMutable` and `GetMutableAll` expose mutable refs/spans and must mark the addressed
  component or chunk dirty before returning storage.
- `EngineWorldServiceAdapter` passes the access object to an `IEngineWorldConsumer`; it
  does not impose a global barrier or own ECS storage.

This is the compile-time contract requested from the future `Delta.ECS` adapter. The concrete
adapter remains responsible for its own consumer-owned dirty tracking.

## Delta.Maths migration map

The current engine-owned vector migration is intentional and layout-compatible:

| Previous type | Current type | Evidence/constraint |
| --- | --- | --- |
| `System.Numerics.Vector2` | `Delta.Maths.float2` | sequential two-float `x/y` struct |
| `System.Numerics.Vector3` | `Delta.Maths.float3` | sequential three-float `x/y/z` struct |
| `System.Numerics.Vector4` | `Delta.Maths.float4` | sequential four-float `x/y/z/w` struct |
| `System.Numerics.Matrix4x4` | `Delta.Maths.float4x4` | column-major 4x4, 64 bytes; `Delta.Maths` tests cover multiplication/layout |
| `System.Numerics.Quaternion` | `Delta.Maths.quaternion` | sequential `(x,y,z,w)`, 16 bytes; `Delta.Maths` tests cover left-handed rotation |

`MaterialData`, `Border`, `Color`, default mesh data, `Transform`, hierarchy matrices,
GPU camera data, and scene/UI GPU records now use `Delta.Maths` primitives. The migration
uses `float4x4.CreateTRS`, `CreateLookTo`, left-handed projection, and quaternion vector
rotation; the conventions are covered by the Maths matrix/quaternion tests. No engine-owned
Matrix/Quaternion duplicate or `System.Numerics` runtime compatibility boundary remains.
