using System;
using System.Collections.Generic;
using System.Text;

internal static class AccessorSourceWriter
{
    public static string Generate(HashSet<Type> types)
    {
        StringBuilder code = new();
        AccessorHeaderWriter.Write(code, types);
        foreach (var type in types)
        {
            AccessorTypeWriter.Write(code, type);
        }

        return code.AppendLine().Append('}').ToString();
    }
}
