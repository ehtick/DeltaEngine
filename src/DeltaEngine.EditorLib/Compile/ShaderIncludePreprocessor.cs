using System;
using System.IO;

internal static class ShaderIncludePreprocessor
{
    public static string Read(string path) => ShaderIncludeFileReader.Read(path);
}
