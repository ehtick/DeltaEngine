using Silk.NET.SPIRV;
using Silk.NET.SPIRV.Cross;
using Delta.Engine.Assets;

internal static class SpirvCrossStageInputReader
{
    public static unsafe VertexAttribute Read(Cross api, Compiler* compiler, ReflectedResource* list, nuint count)
    {
        VertexAttribute result = default;
        for (nuint i = 0; i < count; i++)
        {
            var location = (int)api.CompilerGetDecoration(compiler, list[i].Id, Decoration.Location);
            result |= (VertexAttribute)(1 << location);
        }

        return result;
    }
}
