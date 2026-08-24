# DeltaEngine and DeltaEditor migration inventory

DeltaEditor is already a separate sibling repository in the Furnace workspace.
This document records the remaining ownership/migration boundary; it is not a
future repository-creation plan and contains no transient worktree status.

## Current ownership

| Project | Owner | Boundary |
|---|---|---|
| `Delta.Engine.Integration` | DeltaEngine | neutral lifecycle, scheduling, input, render and module contracts |
| `Delta.Engine` | DeltaEngine | runtime host, scenes/assets and legacy migration adapters |
| `Delta.Engine.Windowed` | DeltaEngine | SDL input/window composition and DeltaRender adapter |
| `Delta.Editor.Scripting` | DeltaEditor | Roslyn compile/diagnostics and collectible load contexts |
| `Delta.Editor.Tooling` | DeltaEditor | project discovery, schema/value adapters and reload policy |
| `DeltaEditorShell` | DeltaEditor workstream | editor controls/views and explicit XAML registry |
| `Delta.Editor.UiHost` | DeltaEditor | adapter from shell/DeltaXAML output to Engine/Render |
| `Delta.Editor.App` | DeltaEditor | only editor composition root |
| legacy `Delta.Engine.Editor*` projects | migration-only | Avalonia/Arch-era source; not a target contract |

## Dependency direction

```text
DeltaEngine neutral contracts
          ^
          |
DeltaEngine runtime/windowed
          ^
          |
DeltaEditor.App
  +-> DeltaEditor.Tooling/Scripting
  +-> DeltaEditorShell -> DeltaXAML
  +-> Delta.Editor.UiHost -> DeltaEngine/DeltaRender
```

DeltaEngine never references DeltaEditor, Roslyn or editor control types.
DeltaEditor may reference published Engine contracts and, where necessary
during migration, runtime adapters. DeltaXAML sees only neutral schema/value
records; ECS/reflection objects stay in DeltaEditor.

## Remaining migration

1. Consolidate the Engine render lifecycle before adding another editor
   adapter. The host-facing contract is `IEngineRenderService); lower sink
   contracts are implementation adapters, not competing public lifecycles.
2. Migrate production UI through the canonical
   `IUiDrawList -> UiRenderBatchAdapter -> UiRenderBatch` chain and retire
   `EngineUiQuad` after its consumers move.
3. Reference `DeltaEditorShell` from `Delta.Editor.App`, register
   `EditorShellLoader`, and use `Delta.Editor.UiHost` only as the
   engine/render bridge.
4. Replace production `EditorInspectableFixtureSource` with the
   DeltaEditor-owned tooling/ECS adapter. Keep fixture sources in tests.
5. Remove remaining legacy editor/runtime reverse references only after the
   replacement path passes headless and bounded native acceptance.

Do not preserve old assembly or namespace identity by copying contracts into a
new owner. Add a temporary adapter only when it is named migration-only and has
a removal condition.

## Acceptance

- architecture tests prove Engine has no Editor/Roslyn/Avalonia dependency;
- the same shell and inspector are instantiated by headless and native app
  paths;
- script reload releases collectible contexts and stale delegates/types;
- close, resize, submission failure and disposal use one lifecycle;
- legacy adapters are listed explicitly and do not appear in stable README API.

The ordered cross-project source of truth is
[../../HIGH_PRIORITY_TODO.md](../../HIGH_PRIORITY_TODO.md); the executable UI
gate is [../../EDITOR_UI_TODO.md](../../EDITOR_UI_TODO.md).
