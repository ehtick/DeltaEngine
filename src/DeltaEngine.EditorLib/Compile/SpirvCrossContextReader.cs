using Silk.NET.SPIRV;
using Silk.NET.SPIRV.Cross;
using Delta.Engine.Assets;
using System;

internal static class SpirvCrossContextReader
{
    public static unsafe VertexAttribute Read(ReadOnlySpan<byte> shaderCode)
    {
        Context* context = default;
        using Cross api = Cross.GetApi();
        api.ContextCreate(&context);
        var result = SpirvCrossCompilerReader.Read(api, context, shaderCode);
        api.ContextDestroy(context);
        return result;
    }
}
