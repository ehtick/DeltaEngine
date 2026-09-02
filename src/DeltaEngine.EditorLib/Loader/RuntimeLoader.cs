using Delta.Engine.Assets;
using Delta.Engine.Assets.Defaults;
using Delta.Engine.Runtime;
using Delta.Engine.EditorLib.Scripting;
using System;
using System.Collections.Generic;
using System.IO;

namespace Delta.Engine.EditorLib.Loader;

[Obsolete("The legacy editor runtime loader is migration-only; use DeltaEditor scripting services and DeltaShader.Tool/codegen for shader assets.", false)]
public sealed class RuntimeLoader : IDisposable
{
    private readonly IProjectPath _projectPath;
    private Delta.Engine.Runtime.Runtime _runtime;

    private readonly CompilerModule _compilerModule;
    private RuntimeScheduler _executionModule;

    private readonly IThreadGetter? _threadGetter;

    public IAccessorsContainer Accessors => _compilerModule.Accessors ??
        throw new InvalidOperationException("Accessor container is unavailable before compiler initialization.");
    public IReadOnlyList<Type> Components => _compilerModule.Components;

    public event EventHandler? OnLoop;


    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Runtime takes ownership of the context returned by RuntimeContextFactory.")]
    public RuntimeLoader(IProjectPath projectPath, IThreadGetter? uiThreadGetter)
    {
        _projectPath = projectPath;
        _threadGetter = uiThreadGetter;

        _compilerModule = new CompilerModule(_projectPath);
        _compilerModule.Recompile();

        var ctx = RuntimeContextFactory.CreateHeadlessContext(_projectPath);
        _runtime = new Delta.Engine.Runtime.Runtime(ctx);

        _executionModule = new RuntimeScheduler(_runtime, _threadGetter);
        _executionModule.OnLoop += ForwardLoop;
        var directory = Directory.GetCurrentDirectory();

        DefaultsImporter<MeshData>.Import(Path.Combine(directory, "Import", "Models"));
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "Runtime takes ownership of the context returned by RuntimeContextFactory.")]
    public void ReloadRuntime()
    {
        _executionModule.Dispose();
        _runtime.Dispose();

        _compilerModule.Recompile();

        var ctx = RuntimeContextFactory.CreateHeadlessContext(_projectPath);
        _runtime = new Delta.Engine.Runtime.Runtime(ctx);
        _executionModule = new RuntimeScheduler(_runtime, _threadGetter);
        _executionModule.OnLoop += ForwardLoop;
    }

    public void Init() => _executionModule.Init();

    private void ForwardLoop(object? sender, EventArgs e) => OnLoop?.Invoke(this, e);

    public void Dispose()
    {
        _executionModule.Dispose();
        _runtime.Dispose();
    }
}
