using Silk.NET.Shaderc;
using System;
using System.Diagnostics;

internal static class ShadercResultReader
{
    public static unsafe byte[] Read(Shaderc api, Compiler* compiler, CompilationResult* result)
    {
        Debug.Assert(api.ResultGetCompilationStatus(result) == CompilationStatus.Success,
            "Shader compilation failed", api.ResultGetErrorMessageS(result));
        byte[] spv = new Span<byte>(api.ResultGetBytes(result), (int)api.ResultGetLength(result)).ToArray();
        api.ResultRelease(result);
        api.CompilerRelease(compiler);
        return spv;
    }
}
