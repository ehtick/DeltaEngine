using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>Writes deterministic imports for generated accessor source.</summary>
/// <remarks>The generated file owns its dependency imports so reflection order does not affect source output.</remarks>
internal static class AccessorHeaderUsingWriter
{
    /// <summary>Appends required and discovered namespaces once.</summary>
    public static void Write(StringBuilder code, HashSet<string> namespaces)
    {
        namespaces.Add("System.Collections.Generic");
        namespaces.Add("System.Runtime.CompilerServices");
        namespaces.Add("System.Collections.Frozen");
        namespaces.Add("Delta.Engine.EditorLib.Scripting");
        code.AppendJoin('\n', namespaces.Distinct().Select(static name => $"using {name};")).AppendLine();
    }
}
