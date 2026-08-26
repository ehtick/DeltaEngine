# DeltaEngine architecture roadmap

This document is the current source of truth for the engine integration work.
The existing source remains a reference and migration target; new architecture
must be introduced through explicit module boundaries rather than a destructive
rewrite.

## Fixed decisions

- Remove Avalonia from the target architecture.
- Use SDL3-CS for desktop windows, input, display/DPI, clipboard, and Vulkan
  surface creation.
- Use one Vulkan renderer on Windows and Linux and MoltenVK over Metal on macOS.
- Use the same DeltaRender pipeline for editor, game, viewports, and runtime UI.
- Use a Delta-owned XAML dialect for UI authoring.
- Replace Arch with DeltaECS after the standalone ECS passes correctness and
  performance gates.
- Use DeltaShader and DeltaMaths as the shader authoring/math path.

## Dependency direction

```text
DeltaMathsGen -> DeltaMaths -> DeltaShader -> SPIR-V + binary ShaderAbi
                                      \          /
DeltaXAML.Contract -> UiDisplayList -> DeltaRender
DeltaText -> positioned glyph data ----/
DeltaECS ---------------------------> DeltaEngine
                                           ^
                         DeltaEditorShell -> DeltaEditor
```

DeltaEngine may depend on DeltaECS, DeltaRender,
`DeltaShader.Contract` runtime contracts, DeltaXAML/DeltaText neutral
contracts and DeltaMaths. The current optional compute/windowed adapters still
use a pre-final shader compatibility path; that is migration state, not the
target dependency. Those
standalone projects must not depend on DeltaEngine. DeltaEditor owns the
application composition and may depend on Engine; the reverse edge is
forbidden.

## Workstream ownership

- `DeltaECS/`: storage, queries, structural changes and measured ECS
  performance work.
- `DeltaXAML/`: XAML dialect, retained controls/tree, properties, layout,
  focus/input and renderer-neutral draw lists.
- `DeltaText/`: shaping, positioned glyphs, outlines and CPU SDF/MSDF data.
- `DeltaRender/`: SDL3 surface/window integration, Vulkan/MoltenVK, render
  graph, GPU UI/text resources and submission.
- `DeltaShader/`: shader compiler and analyzers; it lowers authoring/reflection
  state to final SPIR-V plus the binary ABI owned by `DeltaShader.Contract`.
- `DeltaEngine/`: runtime composition, scenes, assets, serialization, module
  lifecycle, SDL input translation, scheduling and migration adapters.
- `DeltaEditorShell/`: editor controls/views and XAML registry.
- `DeltaEditor/`: Roslyn tooling, ECS inspection, UiHost adapters and the
  editor application composition root.

Do not edit another workstream's new project to unblock local work. Define a
small interface or fixture and report the required contract to its owner.

## Engine responsibilities

The engine owns:

- process and module lifecycle;
- deterministic update stages and service composition;
- scene/prefab/asset identities and serialization;
- asset import/build/cache pipeline;
- neutral user assembly/module loading boundaries; Roslyn compilation remains
  in DeltaEditor;
- game host configuration and editor-facing runtime services;
- adapters during migration from Arch, Avalonia, and the current renderer;
- diagnostics, logging, crash context, and performance telemetry.

It does not own Vulkan calls, SDL calls, ECS storage internals, shader lowering,
or XAML layout/rendering.

## Migration approach

1. Inventory current dependencies and produce a build/test baseline for the
   existing `DeltaEngine/Source` solution.
2. Define narrow engine-facing contracts for world/runtime, rendering/extraction,
   input snapshots, UI documents, assets, clocks, jobs, and diagnostics.
3. Add a small new host/composition project without deleting the existing
   editor or runtime.
4. Integrate standalone DeltaRender's first surface/present sample.
5. Add an Arch adapter behind the world contract and migrate systems only after
   DeltaECS supplies an equivalent adapter.
6. Replace Avalonia editor panels with Delta XAML controls incrementally. The
   Vulkan viewport remains a GPU image and is composited directly.
7. Move shader assets to `DeltaShader.Contract` artifacts (SPIR-V plus binary
   `ShaderAbi`). GLSL, Roslyn/reflection state, live generic values and content
   hashes do not enter the runtime artifact.
8. Delete old adapters and dependencies only after feature and test parity.

No phase should require a GPU-to-CPU copy to display the editor viewport.

## Platform policy

Desktop support means macOS arm64/x64, Windows x64/arm64, and Linux x64/arm64.
Capabilities are discovered at runtime and represented in diagnostics. Apple
portability requirements stay inside DeltaRender. Engine code must not branch on
MoltenVK except for user-facing diagnostics and packaging.

CI should contain platform-neutral unit tests plus native smoke jobs for SDL and
Vulkan. A platform is not considered supported solely because it compiles.

## First engine delivery

The first delivery is architectural and executable:

- document the current dependency/migration inventory;
- create engine-facing interfaces with no SDL, Vulkan, Avalonia, or Arch types
  in their signatures;
- create a minimal host that runs deterministic lifecycle/update stages with
  fake world, render, input, and UI services;
- add contract tests for startup, stage order, shutdown, exception propagation,
  and module disposal;
- add adapters only where required to demonstrate coexistence with the current
  source tree;
- provide explicit contract requests to the ECS and renderer owners.

The concrete stage/interface details belong in the project `USER_API.md` and
`INTERNAL.md`; this roadmap records only the durable direction.

Do not begin by deleting Avalonia or Arch packages. Their removal is an outcome
of successful migration, not the first step.

## Quality gates

- New engine code builds on all supported desktop target frameworks/runtimes.
- Module boundaries are enforced by project references and architecture tests.
- Lifecycle and stage-order tests are deterministic.
- No new dependency on Avalonia or Arch is introduced.
- No render result is copied to CPU for UI composition.
- Hot update/extraction paths have allocation benchmarks.
- Every platform-specific native dependency has packaging and smoke-test notes.
