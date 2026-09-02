using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

internal static class AccessorFieldGetterWriter
{
    public static void Write(StringBuilder code, IEnumerable<FieldInfo> fields, Type type)
    {
        code.AppendLine().Append("public object GetFieldValue(ref readonly object obj, string name)").AppendLine().Append('{').AppendLine();
        code.Append("switch (name)").AppendLine().Append('{').AppendLine();
        code.AppendJoin('\n', fields.Select(field =>
            $"case \"{field.Name}\":\n{{\nvar val = ({AccessorNameFormatter.GetFormattedName(type)})obj;\nreturn {AccessorNameFormatter.GetSetMethodName(field)}(ref val);\n}}"))
            .AppendLine();

        code.Append("default: throw new InvalidOperationException($\"Field with name {name} of type {typeof(Transform)} not found\");")
            .AppendLine().Append("};").AppendLine().Append('}').AppendLine();
    }
}
