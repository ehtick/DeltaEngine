using System;
using System.Collections.Generic;
using System.Text;

internal static class AccessorHeaderWriter
{
    public static void Write(StringBuilder code, HashSet<Type> types) =>
        AccessorHeaderContentWriter.Write(code, types);
}
