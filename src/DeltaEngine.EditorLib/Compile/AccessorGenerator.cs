using System;
using System.Collections.Generic;

// TODO: Rewrite with templates as Delta.Engine.Generation
// Add support for generics components
internal static class AccessorGenerator
{
    public static string GenerateAccessors(HashSet<Type> componentTypes) =>
        AccessorSourceWriter.Generate(AccessorTypeGraph.Collect(componentTypes));

    public static void GetAvaliableTypes(Type type, HashSet<Type> visited) =>
        AccessorTypeGraph.Add(type, visited);
}
