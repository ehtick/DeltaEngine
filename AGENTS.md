# DeltaEngine agent router

Scope: runtime scheduling, SDL event/input translation, scene orchestration and
neutral adapters. Editor-specific Roslyn/tooling belongs in DeltaEditor.
Headless integration stays independent of SDL/Vulkan; renderer never polls
input, and Engine never parses shader source, XAML or font outlines.

## Map — open only as needed

- ../CODE_STYLE.md — technical lifecycle, ownership, determinism and scheduling rules.
- ../CONTRACTS.md — cross-project ownership; open only for a boundary task.
- IDEAS.md — runtime research/options only when requested.
- WORKFLOW.md — targeted builds, tests and integration checks.
- USER_API.md — user-facing runtime API; open only for public API/documentation work.
- INTERNAL.md and docs/architecture-roadmap.md — implementation boundaries and durable direction.
- src/DeltaEngine and src/DeltaEngine.* — production runtime and implementation siblings.
- tests, probes, samples — verification, focused checks and runnable leaves.

Use game-developer, static-analysis, concurrency-debugging, apple-silicon and
lldb only for the matching bounded area.
