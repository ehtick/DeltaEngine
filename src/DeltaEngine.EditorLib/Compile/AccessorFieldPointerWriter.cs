using System;
using System.Collections.Generic;
using System.Linq;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

internal static class AccessorFieldPointerWriter
{
    public static void Write(StringBuilder code, IEnumerable<FieldInfo> fields, Type type)
    {
        code.AppendLine().Append("public unsafe nint GetFieldPtr(nint ptr, string name)").AppendLine().Append('{').AppendLine();
        code.Append(CultureInfo.InvariantCulture, $"ref var obj = ref Unsafe.AsRef<{AccessorNameFormatter.GetFormattedName(type)}>(ptr.ToPointer());").AppendLine();
        code.Append("return name switch").AppendLine().Append('{').AppendLine();
        code.AppendJoin('\n', fields.Select(field =>
            $"\"{field.Name}\"=> new nint(Unsafe.AsPointer(ref {AccessorNameFormatter.GetSetMethodName(field)}(ref obj))),"))
            .AppendLine();

        code.Append("_ => throw new InvalidOperationException($\"Field with name {name} of type {typeof(Transform)} not found\")")
            .AppendLine().Append("};").AppendLine().Append('}').AppendLine();
    }
}
