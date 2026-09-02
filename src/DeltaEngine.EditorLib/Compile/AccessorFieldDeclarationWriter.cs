using System;
using System.Reflection;
using System.Text;

internal static class AccessorFieldDeclarationWriter
{
    public static void Write(StringBuilder code, FieldInfo field, Type container)
    {
        code.Append("[UnsafeAccessor(UnsafeAccessorKind.Field, Name = \"").Append(field.Name).Append("\")]\n")
            .Append("public extern static ref ").Append(AccessorNameFormatter.GetFormattedName(field.FieldType)).Append(' ')
            .Append(AccessorNameFormatter.GetSetMethodName(field)).Append("(ref ")
            .Append(AccessorNameFormatter.GetFormattedName(container)).Append(" obj);\n");
    }
}
