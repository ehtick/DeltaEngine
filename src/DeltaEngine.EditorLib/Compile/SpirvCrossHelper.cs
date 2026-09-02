using Silk.NET.SPIRV;
using Delta.Engine.Assets;
using System;

internal static class SpirvCrossHelper
{
    public static unsafe VertexAttribute GetInputAttributes(ReadOnlySpan<byte> shaderCode) =>
        SpirvCrossContextReader.Read(shaderCode);
}
