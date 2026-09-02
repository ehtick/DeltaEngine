using Delta.Engine.Assets;
using Delta.Engine.Runtime;
using Silk.NET.Shaderc;
using System.Collections.Generic;

internal static class ShaderAssetWriter
{
    public static void Write(string name, Dictionary<string, string> files)
    {
        var vertex = ShaderSourceCompiler.Compile(files[".vert"], ShaderKind.VertexShader);
        var fragment = ShaderSourceCompiler.Compile(files[".frag"], ShaderKind.FragmentShader);
        var flags = SpirvCrossHelper.GetInputAttributes(vertex);
        var importer = IRuntimeContext.Current.AssetImporter;
        importer.CreateAsset(new MaterialData(importer.CreateAsset(new ShaderData(vertex, fragment, flags), name + ".shader")), name + "Material.mat");
    }
}
