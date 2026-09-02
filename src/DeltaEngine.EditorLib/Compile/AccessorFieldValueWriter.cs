using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

internal static class AccessorFieldValueWriter
{
    public static void WriteValue(StringBuilder code, IEnumerable<FieldInfo> fields, Type type) =>
        AccessorFieldGetterWriter.Write(code, fields, type);

    public static void WritePointer(StringBuilder code, IEnumerable<FieldInfo> fields, Type type) =>
        AccessorFieldPointerWriter.Write(code, fields, type);
}
