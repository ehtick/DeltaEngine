# DeltaEngine workflow

Build the narrow producer first, then the solution:

```bash
dotnet restore Source/Delta.Engine.slnx
dotnet build Source/Delta.Engine.Windowed/Delta.Engine.Windowed.csproj \
  -c Release --no-restore --disable-build-servers -m:1 \
  /p:UseSharedCompilation=false
dotnet build Source/Delta.Engine.slnx -c Release --no-restore \
  --disable-build-servers -m:1 /p:UseSharedCompilation=false -v:minimal
dotnet test Source/Delta.Engine.slnx -c Release --no-build --no-restore \
  --disable-build-servers -m:1
```

Use headless contract tests before native composition. Surface existing legacy
warnings separately; do not mix dependency upgrades into an integration fix.
`Delta.Engine.Windowed.Tests` includes fault-injection coverage for transactional
initialization, rollback and retry; these tests do not open a native window.
Run the real editor window from [../DeltaEditor/WORKFLOW.md](../DeltaEditor/WORKFLOW.md).

Before building or running the windowed text path, prepare the generated SDF
SPIR-V artifacts from the DeltaShader repository:

```bash
DeltaShader/eng/prepare-text-artifacts.sh DeltaShader/artifacts/text
```

The windowed project copies only `SdfTextVertex.vert.spv` and
`SdfTextFragment.frag.spv`; the generated shader factory owns their embedded
ABI manifests. This is current compatibility packaging for the
`Delta.Shader.Abstractions` consumer. The target runtime handoff is a complete
`Delta.Shader.Contract.IShaderArtifact` (SPIR-V plus binary `ShaderAbi`), not a
raw SPIR-V file, GLSL sidecar or compiler object.

## Code metrics

Run the same analyzer/code-metrics build locally and in the manual GitHub
Actions workflow through the repository wrapper:

```bash
./eng/code-metrics.sh -v:q
```

`eng/code-metrics.sh` converts `CODE_METRICS_ERROR_LOG` (default:
`artifacts/code-metrics/diagnostics.sarif`) to an absolute path before
MSBuild starts, so multi-project builds write one repository-level SARIF
instead of resolving a missing directory relative to each project. An
explicit destination is supported:

```bash
CODE_METRICS_ERROR_LOG=/tmp/code-metrics.sarif ./eng/code-metrics.sh -v:q
```

Inspect the SARIF and summary artifacts from the manual workflow. The rules
CA1501/CA1502/CA1505/CA1506 are report-only signals; do not refactor a method
for one isolated warning. Refactor when several metrics remain over their
limits, the issue persists across runs, or profiling identifies a hot path.

For local application run `./eng/format.sh`; for a non-mutating check use
`FORMAT_CHECK=1 ./eng/format.sh`. The script uses `dotnet format whitespace
--folder` to avoid the MSBuild/Roslyn workspace load that can hang on macOS
with .NET 10. It checks/applies whitespace only; analyzer/style diagnostics
remain covered by the build and SARIF metrics workflow.
