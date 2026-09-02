using System;
using System.Text;

internal static class AccessorTypeContentWriter
{
    public static void Write(StringBuilder code, Type type)
    {
        var fields = AccessorTypeGraph.SelectFields(type.GetFields());
        code.Append("private class ").Append(AccessorNameFormatter.GetAccessorName(type))
            .Append(": IAccessor").AppendLine().Append('{').AppendLine();
        AccessorFieldGroupWriter.Write(code, fields, type);
        code.AppendLine().Append('}').AppendLine();
    }
}
