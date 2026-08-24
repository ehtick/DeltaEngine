# DeltaEngine

Runtime composition layer for the Furnace stack. It owns frame timing, SDL
event polling and input translation, close/resize handling, scene/runtime
orchestration and neutral subsystem adapters.

```text
Engine host -> SDL input -> DeltaECS/user update -> DeltaXAML update/layout
  -> canonical UI/render submission -> DeltaRender present
```

Headless `Delta.Engine.Integration` remains independent of SDL, Vulkan,
DeltaRender and DeltaShader. DeltaEngine does not own Vulkan resources, shader
compilation, XAML parsing/layout, font shaping or editor Roslyn tooling.
DeltaEditor owns scripting and inspection.
`DeltaEditorShell` owns editor controls/views; `Delta.Editor.UiHost` is the
adapter and `Delta.Editor.App` is the composition root.

Avalonia and Arch are migration-only dependencies; new runtime work must not
deepen them. Durable dependency direction is documented in
[docs/architecture-roadmap.md](docs/architecture-roadmap.md).

See [WORKFLOW.md](WORKFLOW.md) for targeted verification,
[TODO.md](TODO.md) for selected work and [AGENTS.md](AGENTS.md) for routing.
