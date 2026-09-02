using Silk.NET.SPIRV;
using Silk.NET.SPIRV.Cross;
using Delta.Engine.Assets;
using System;

internal static class SpirvCrossCompilerReader
{
    public static unsafe VertexAttribute Read(Cross api, Context* context, ReadOnlySpan<byte> shaderCode)
    {
        ParsedIr* ir = default;
        Compiler* compiler = default;
        Resources* resources = default;
        Set set = default;
        ReflectedResource* list = default;
        nuint count = default;
        fixed (void* decodedPtr = shaderCode)
        {
            api.ContextParseSpirv(context, (uint*)decodedPtr, (uint)shaderCode.Length / 4, &ir);
            api.ContextCreateCompiler(context, Backend.None, ir, CaptureMode.TakeOwnership, &compiler);
            api.CompilerGetActiveInterfaceVariables(compiler, &set);
            api.CompilerCreateShaderResources(compiler, &resources);
            api.ResourcesGetResourceListForType(resources, ResourceType.StageInput, &list, &count);
        }

        return SpirvCrossStageInputReader.Read(api, compiler, list, count);
    }
}
