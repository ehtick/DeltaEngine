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
Run the real editor window from [../DeltaEditor/WORKFLOW.md](../DeltaEditor/WORKFLOW.md).

## Code metrics

Run the manual GitHub Actions `Code metrics` workflow before committing a
substantial change, then inspect its SARIF and summary artifacts. The rules
CA1501/CA1502/CA1505/CA1506 are report-only signals; do not refactor a method
for one isolated warning. Refactor when several metrics remain over their
limits, the issue persists across runs, or profiling identifies a hot path.

For local application run `./eng/format.sh`; for a non-mutating check use
`FORMAT_CHECK=1 ./eng/format.sh`. Run the check before committing substantial
changes. The script uses the repository `.editorconfig` and `Directory.Build.props`.
