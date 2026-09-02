# DeltaEngine

DeltaEngine provides a deterministic host for game and editor runtime stages,
with headless execution and optional platform/render integrations.

## What it provides

- Deterministic initialization, frame, shutdown and disposal order.
- Platform-neutral input delivery to the host.
- Explicit world, UI and rendering stage composition.
- A renderer-neutral frame boundary for game and editor features.
- `NullRenderer` for headless runs without native graphics resources.
- Optional SDL-based window and input integration through the Windowed project.

## Quick start

```xml
<PackageReference Include="DeltaEngine" Version="*" />
```

```csharp
using Delta.Engine.Integration;

var renderer = new NullRenderer();
renderer.Initialize();
renderer.Render(new EngineRenderFrame(0, default));

Console.WriteLine(renderer.BackendName); // none
renderer.Dispose();
```

The example submits one frame without opening a window or requiring a graphics
backend.

## Core concepts

```text
input -> world -> UI -> render
```

`EngineHost` owns stage ordering. Services receive neutral values and retain
their own platform, world, UI or renderer resources. Time and simulation data
are supplied by the caller rather than stored in the render contract.

## Capabilities and limits

- Headless execution is supported through neutral services and `NullRenderer`.
- Windowed execution is an optional SDL/MoltenVK composition.
- Engine contracts do not expose platform, graphics backend, compiler or
  storage internals.
- Shader compilation, XAML layout and ECS storage remain owned by their
  respective projects.
- No standalone runnable samples are currently shipped in this repository.

## Packages and examples

- `DeltaEngine` is the runtime package.
- The Windowed project is used when an application needs SDL input and a
  native render surface.
- See the [user API](USER_API.md) for host lifecycle and ownership details.

## Further reading

- [User API](USER_API.md)
- [Cross-project contracts](../CONTRACTS.md)
- [Architecture overview](docs/architecture-roadmap.md)
