# DeltaEngine

Runtime composition layer for the Furnace stack. It owns frame timing, SDL
event polling and input translation, close/resize handling, scene/runtime
orchestration and neutral subsystem adapters.

Time is an Engine-owned scheduling input. ECS lifecycle contracts are
parameterless, and renderer-facing frames contain frame identity and surface
state only. Fixed-step, scaled, unscaled and editor clocks are selected by the
host or a feature extractor and passed as explicit system/shader data; neither
DeltaECS nor DeltaRender owns a global `DeltaTime`.

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

Optional windowed/compute adapters currently consume
`Delta.Shader.Abstractions.ShaderArtifact` through DeltaRender as a migration
compatibility path. The final runtime boundary is the immutable
`Delta.Shader.Contract.IShaderArtifact`: SPIR-V plus resolved binary
`ShaderAbi`. Engine does not transport GLSL, Roslyn/compiler state, live
generic shader values or an artifact-owned content hash.

The optional windowed renderer initializes its session and pipelines
transactionally: failed initialization rolls back resources created so far,
leaves the service uninitialized for a retry, and repeated successful
`Initialize`/`Dispose` calls are idempotent.

Avalonia and Arch are migration-only dependencies; new runtime work must not
deepen them. Durable dependency direction is documented in
[docs/architecture-roadmap.md](docs/architecture-roadmap.md).

See [WORKFLOW.md](WORKFLOW.md) for targeted verification,
[TODO.md](TODO.md) for selected work and [AGENTS.md](AGENTS.md) for routing.
