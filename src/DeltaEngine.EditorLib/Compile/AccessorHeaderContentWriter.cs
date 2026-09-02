using Delta.Engine.EditorLib.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

internal static class AccessorHeaderContentWriter
{
    public static void Write(StringBuilder code, HashSet<Type> types)
    {
        var fields = AccessorTypeGraph.SelectFields(types.SelectMany(static type => type.GetFields()));
        var namespaces = fields.Select(static field => field.FieldType.Namespace)
            .Concat(types.Select(static type => type.Namespace))
            .OfType<string>()
            .ToHashSet();
        AccessorHeaderUsingWriter.Write(code, namespaces);
        code.Append($"public class AccessorsContainer: {nameof(IAccessorsContainer)}")
            .AppendLine().Append('{').AppendLine();
        code.Append($"public FrozenDictionary<Type, {nameof(IAccessor)}> AllAccessors ")
            .Append("{ get; } = new Dictionary<Type, IAccessor>()").AppendLine().Append('{').AppendLine();
        code.AppendJoin('\n', types.Select(static type =>
            $"{{typeof({AccessorNameFormatter.GetFormattedName(type)}), new {AccessorNameFormatter.GetAccessorName(type)}()}},"))
            .AppendLine();
        code.Append("}.ToFrozenDictionary();").AppendLine();
    }
}
