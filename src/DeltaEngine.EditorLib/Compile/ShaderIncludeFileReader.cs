using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal static class ShaderIncludeFileReader
{
    public static string Read(string path)
    {
        string directory = Path.GetDirectoryName(path) ??
            throw new ArgumentException("Shader path must include a directory.", nameof(path));
        StringBuilder preprocessed = new();
        HashSet<string> includes = [];
        foreach (var line in File.ReadLines(path))
        {
            ShaderIncludeLineWriter.Append(preprocessed, directory, line, includes);
        }

        return preprocessed.ToString();
    }
}
