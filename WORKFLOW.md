# DeltaEngine workflow

## Benchmark parameter policy

BenchmarkDotNet attributes may describe benchmark methods, categories and
lifecycle hooks, but they must not define workload or run parameters. Do not add
`[Params]`, `[ParamsSource]`, `[Arguments]`, `[ArgumentsSource]` or equivalent
parameter attributes. Parse every workload/configuration value from application
command-line arguments (or the invoking script) before BenchmarkDotNet starts,
and pass the resulting values into the benchmark runner. Keep BDN runner
switches such as `--filter` and `--job` separate from workload input. Existing
parameter attributes are migration debt: do not add new uses and replace them
when that benchmark is next modified.


## Repository layout gate

The repository must follow the shared first-party layout documented in the
Furnace project standard. Before restore/build or a structural handoff, run:

```bash
./eng/check-layout.sh
```

The gate checks the mandatory top-level directories, rejects unexpected
tracked top-level folders, requires src/DeltaEngine/ as the primary source
project, and requires source siblings to use the src/DeltaEngine.<Area>/ form.
samples/ contains runnable examples; probes/ contains bounded
headless/compiler/contract checks. Empty mandatory domains stay tracked with
.gitkeep.

## Delta runtime namespace gate

First-party C# code uses the canonical managed spelling `Maths.*` with
`using Delta;`. Do not use direct `System.Math`, `MathF`, `DeltaMaths.*`
or the lowercase provider spelling. Run the bounded gate before handing off
math-related changes:

```bash
./eng/check-no-system-math.sh
```

The gate scans first-party C# roots and excludes only generated `obj`/`bin`
output. No provider-file exceptions are currently allowed.

Build the narrow producer first, then the solution:

```bash
dotnet restore src/DeltaEngine.slnx
dotnet build src/DeltaEngine.Windowed/DeltaEngine.Windowed.csproj \
  -c Release --no-restore --disable-build-servers -m:1 \
  /p:UseSharedCompilation=false
dotnet build src/DeltaEngine.slnx -c Release --no-restore \
  --disable-build-servers -m:1 /p:UseSharedCompilation=false -v:minimal
dotnet test src/DeltaEngine.slnx -c Release --no-build --no-restore \
  --disable-build-servers -m:1
```

Use headless contract tests before native composition. Surface existing legacy
warnings separately; do not mix dependency upgrades into an integration fix.
`DeltaEngine.Windowed.Tests` includes fault-injection coverage for transactional
initialization, rollback and retry; these tests do not open a native window.
Run the real editor window from [../DeltaEditor/WORKFLOW.md](../DeltaEditor/WORKFLOW.md).

Before building or running the windowed text path, build the DeltaShader text
producer project normally. Its private `DeltaShader.Tool` NuGet reference
generates the program/factory surface and the final typed artifact API. The
runtime must receive that producer output through the normal artifact handoff.
The target runtime handoff is a complete
`DeltaShader.Contract.IShaderArtifact` (SPIR-V plus binary `ShaderAbi`), not a
raw SPIR-V file, GLSL sidecar or compiler object.
Run `./eng/check-shader-output-ownership.sh` to reject Engine-local generated
shader binaries and sidecars.

## Code metrics

Run the shared Furnace wrappers from this repository before every commit; see
the [common workflow](../REVIEW_PLAYBOOK.md#shared-local-formatter-and-metrics-wrappers):

```bash
../eng/format.sh "$PWD"
FORMAT_CHECK=1 ../eng/format.sh "$PWD"
../eng/code-metrics.sh "$PWD" -v:q
```

Set `CODE_METRICS_ERROR_LOG` when a different SARIF destination is needed:

```bash
CODE_METRICS_ERROR_LOG=/tmp/deltaengine-metrics.sarif \
  ../eng/code-metrics.sh "$PWD" -v:q
```

Inspect the SARIF and summary artifacts from the manual workflow. The rules
CA1501/CA1502/CA1505/CA1506 are report-only signals; do not refactor a method
for one isolated warning. Refactor when several metrics remain over their
limits, the issue persists across runs, or profiling identifies a hot path.
