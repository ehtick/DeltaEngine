using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

internal static class AccessorFieldGroupWriter
{
    public static void Write(StringBuilder code, IEnumerable<FieldInfo> fields, Type type)
    {
        AccessorFieldMetadataWriter.WriteNames(code, fields);
        AccessorFieldValueWriter.WriteValue(code, fields, type);
        AccessorFieldMetadataWriter.WriteType(code, fields);
        AccessorFieldValueWriter.WritePointer(code, fields, type);
        foreach (var field in fields)
        {
            AccessorFieldDeclarationWriter.Write(code, field, type);
        }
    }
}
