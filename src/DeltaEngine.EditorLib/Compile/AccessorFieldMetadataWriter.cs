using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

internal static class AccessorFieldMetadataWriter
{
    public static void WriteNames(StringBuilder code, IEnumerable<FieldInfo> fields)
    {
        code.AppendLine().Append("private readonly string[] _fieldNames = [");
        code.AppendJoin('\n', fields.Select(static field => $"\"{field.Name}\","))
            .AppendLine("];public ReadOnlySpan<string> FieldNames => new(_fieldNames);");
    }

    public static void WriteType(StringBuilder code, IEnumerable<FieldInfo> fields)
    {
        code.AppendLine().Append("public Type GetFieldType(string name)").AppendLine().Append('{').AppendLine();
        code.Append("return name switch").AppendLine().Append('{').AppendLine();
        code.AppendJoin('\n', fields.Select(field =>
            $"\"{field.Name}\"=> typeof({AccessorNameFormatter.GetFormattedName(field.FieldType)}),"))
            .AppendLine();

        code.Append("_ => throw new InvalidOperationException($\"Field with name {name} of type {typeof(Transform)} not found\")")
            .AppendLine().Append("};").AppendLine().Append('}').AppendLine();
    }
}
