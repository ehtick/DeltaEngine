using Silk.NET.Shaderc;
using System;

internal static unsafe class ShaderSourceCompiler
{
    public static byte[] Compile(string path, ShaderKind shaderKind) =>
        ShadercCompiler.Compile(path, shaderKind);
}
