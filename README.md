# DeltaEngine

Runtime composition for deterministic host lifecycle, platform-neutral input,
world orchestration and renderer-neutral frame submission.

- [USER_API.md](USER_API.md) - stable host-facing API and ownership boundaries.
- [INTERNAL.md](INTERNAL.md) - migration adapters and implementation notes.
- [WORKFLOW.md](WORKFLOW.md) - bounded build, test, format and metrics commands.
- [TODO.md](TODO.md) - selected engine work.
- [docs/architecture-roadmap.md](docs/architecture-roadmap.md) - durable
  dependency direction and migration policy.

DeltaEngine core does not own Vulkan resources, ECS storage, shader compilation,
XAML layout or editor Roslyn tooling. Its optional Windowed adapter owns SDL
event polling and translates events into the canonical UI input contract.
