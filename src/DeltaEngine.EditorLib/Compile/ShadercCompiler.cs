using Silk.NET.Shaderc;
using System;
using System.Diagnostics;
using System.IO;

internal static class ShadercCompiler
{
    // This backend is intentionally isolated from runtime code.
    // Shaderc owns the native compiler handles and emits SPIR-V bytes.
    // Includes are expanded before entering the native API.
    // The result reader releases both handles after copying the bytes.
    // This path is cold and runs only during editor asset import.
    public static unsafe byte[] Compile(string path, ShaderKind shaderKind)
    {
        using var api = Shaderc.GetApi();
        var options = api.CompileOptionsInitialize();
        api.CompileOptionsSetOptimizationLevel(options, OptimizationLevel.Performance);
        Compiler* compiler = api.CompilerInitialize();
        var name = Path.GetFileNameWithoutExtension(path);
        var source = ShaderIncludePreprocessor.Read(path);
        Debug.WriteLine(source, "Shader compiler");
        CompilationResult* result = api.CompileIntoSpv(compiler, source, (nuint)source.Length, shaderKind, name, "main", options);
        return ShadercResultReader.Read(api, compiler, result);
    }
}
