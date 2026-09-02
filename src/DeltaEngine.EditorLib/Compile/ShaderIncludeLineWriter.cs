using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

internal static class ShaderIncludeLineWriter
{
    public static void Append(StringBuilder output, string directory, string line, HashSet<string> includes)
    {
        const string includeKeyword = "#include";
        if (!line.StartsWith(includeKeyword, StringComparison.Ordinal))
        {
            output.AppendLine(line);
            return;
        }

        var includePath = line[includeKeyword.Length..]
            .Replace(" ", string.Empty, StringComparison.Ordinal)
            .Replace("\"", string.Empty, StringComparison.Ordinal);
        if (includes.Add(includePath))
        {
            output.Append(File.ReadAllText(Path.Combine(directory, includePath)));
        }
    }
}
