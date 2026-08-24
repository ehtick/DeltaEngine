# DeltaEngine TODO

- [x] Make optional windowed renderer initialization transactional with
  deterministic rollback and retry-safe idempotent lifecycle.
- After upstream Shader/Render migration, consolidate the host-facing render
  lifecycle on `IEngineRenderService`; lower sink contracts become internal
  adapters rather than competing public lifecycles.
- Define neutral engine-user component/system discovery and lifecycle contracts
  without Roslyn, reflection objects or editor types in runtime assemblies.
- Schedule user systems, selected-entity inspection, DeltaXAML update/layout
  and DeltaRender submission in a deterministic same-frame order. Migrate the
  current render-before-UI historical order to UI-before-render with its
  stage-order tests.
- Complete close/resize/disposal acceptance for the real editor application.
- Move storage-specific reads/writes and spans to the ECS adapter; keep the
  copied world-change journal migration-only.

Execution order is in
[../HIGH_PRIORITY_TODO.md](../HIGH_PRIORITY_TODO.md); shared inspector gates
are in [../EDITOR_UI_TODO.md](../EDITOR_UI_TODO.md).
