using System;
using System.Collections.Generic;
using System.IO;

namespace DeltaEngine.Integration;

public readonly record struct ScriptSource(string Path, string Text);

public interface IScriptReference
{
    string Name { get; }
    Stream OpenRead();
}

public sealed record ScriptCompilationRequest(
    IReadOnlyList<ScriptSource> Sources,
    IReadOnlyList<IScriptReference> References,
    string AssemblyName = "Scripts");

public enum ScriptDiagnosticSeverity
{
    Hidden,
    Info,
    Warning,
    Error,
}

public sealed record ScriptCompilationDiagnostic(
    string Id,
    ScriptDiagnosticSeverity Severity,
    string Message,
    string? SourcePath,
    int? Line,
    int? Column);

public sealed record ScriptCompilationResult(
    bool Success,
    ReadOnlyMemory<byte> AssemblyBytes,
    ReadOnlyMemory<byte> PdbBytes,
    IReadOnlyList<ScriptCompilationDiagnostic> Diagnostics);

public interface IScriptCompiler
{
    ScriptCompilationResult Compile(ScriptCompilationRequest request);
}
