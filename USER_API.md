# DeltaEngine user API

This file describes the public runtime boundary. It is a user-facing API
document, not an implementation or migration plan.

## Host lifecycle

`EngineHost` composes `IEngineInputService`, `IEngineWorldService`,
`IEngineUiService` and `IEngineRenderService` with deterministic lifecycle:

```text
initialize: Input -> World -> Render -> UI
frame:      Input -> World -> UI -> Render
shutdown:   UI -> Render -> World -> Input
dispose:    UI -> Render -> World -> Input
```

Exceptions propagate to the caller. The host does not poll input from the
renderer and does not schedule ECS internals.

## Renderer boundary

`EngineRenderFrame` is time-free and contains frame identity plus surface
state. `IEngineRenderService.Render` is the only host-facing render
submission method. Time domains and simulation data remain owned by the host
or by feature adapters.

`NullRenderer` implements the same service contract for headless runs. It has
no backend and records only lifecycle observations useful to tests.

## Ownership

Engine-facing contracts contain no SDL, Vulkan, Avalonia, Arch, Roslyn or ECS
storage types. Platform, renderer, ECS and editor implementations are supplied
through adapters owned by their respective projects.
