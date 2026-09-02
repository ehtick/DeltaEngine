using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;

internal static class ShaderModuleCatalog
{
    private const string VertexExtension = ".vert";
    private const string FragmentExtension = ".frag";

    public static Dictionary<string, Dictionary<string, string>> Read(string directory)
    {
        return Directory.EnumerateFiles(directory)
            .GroupBy(path => Path.GetFileNameWithoutExtension(path) ?? string.Empty)
            .ToDictionary(group => group.Key, group => group.ToDictionary(
                path => Path.GetExtension(path) ?? throw new InvalidOperationException("Shader file has no extension."),
                path => path));
    }

    public static bool IsGraphicsModule(Dictionary<string, string> files) =>
        files.Count == 2 && files.ContainsKey(VertexExtension) && files.ContainsKey(FragmentExtension);
}
