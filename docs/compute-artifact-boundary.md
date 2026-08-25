# Compute artifact boundary

`Source/Delta.Engine.ComputeSmoke` owns orchestration only. It loads the
generated SPIR-V and ABI manifest pair into the runtime-neutral
`Delta.Shader.Abstractions.ShaderArtifact`; this is the current compatibility
path, not the final artifact contract. It does not duplicate shader metadata or
renderer pipeline contracts.

The sample does not reference `Delta.Shader.Compiler`, Roslyn, MSBuild, or
shader-generator runtime APIs. `compute_double.spv` is a pre-generated fixture;
the CPU world update, renderer subscription, SSBO upload, dispatch, readback,
and `i * 2 + 1` oracle remain runnable without shader compilation at runtime.

`Delta.Render` currently consumes that compatibility object through
`IComputeDevice.CreateComputePipeline(ShaderArtifact)` and derives renderer
metadata from its legacy manifest. The target API consumes
`Delta.Shader.Contract.IShaderArtifact`, whose complete handoff is SPIR-V plus
the resolved binary `ShaderAbi`. The checked-in pair is generated output for
the smoke path; it is not the target runtime artifact. Runtime Engine does not
reference Roslyn, MSBuild, the shader compiler, GLSL, live generic shader
values or an artifact-owned content hash.
