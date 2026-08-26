using DeltaEngine.Assets;
using DeltaEngine.Assets.Defaults;
using DeltaEngine.Runtime;
using DeltaEngine.EditorLib.Compile;
using DeltaEngine.EditorLib.Scripting;
using System;
using System.Collections.Generic;
using System.IO;

namespace DeltaEngine.EditorLib.Loader;

public sealed class RuntimeLoader : IDisposable
{
    private readonly IProjectPath _projectPath;
    private DeltaEngine.Runtime.Runtime _runtime;

    private readonly CompilerModule _compilerModule;
    private readonly ShaderCompilerModule _shaderCompilerModule;
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
        _shaderCompilerModule = new ShaderCompilerModule();

        _compilerModule.Recompile();

        var ctx = RuntimeContextFactory.CreateHeadlessContext(_projectPath);
        _runtime = new DeltaEngine.Runtime.Runtime(ctx);

        _executionModule = new RuntimeScheduler(_runtime, _threadGetter);
        _executionModule.OnLoop += ForwardLoop;
        var directory = Directory.GetCurrentDirectory();

        DefaultsImporter<MeshData>.Import(Path.Combine(directory, "Import", "Models"));
        _shaderCompilerModule.CompileAndImportShaders(Path.Combine(directory, "Import", "Shaders"));
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
        _runtime = new DeltaEngine.Runtime.Runtime(ctx);
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
