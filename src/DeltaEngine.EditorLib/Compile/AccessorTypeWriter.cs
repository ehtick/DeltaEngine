using System;
using System.Text;

internal static class AccessorTypeWriter
{
    public static void Write(StringBuilder code, Type type) =>
        AccessorTypeContentWriter.Write(code, type);
}
