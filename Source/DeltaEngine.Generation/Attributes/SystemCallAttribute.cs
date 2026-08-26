using DeltaEngine.Generation.Core;

namespace DeltaEngine.Generation.Attributes;

internal class SystemCallAttribute : AttributeTemplate
{
    public override string Name => nameof(SystemCallAttribute);
    public override string ToString() =>
$$"""
#if {{Constants.GenerateAttributes}}


namespace DeltaEngine;

[System.AttributeUsage(System.AttributeTargets.Method)]
public sealed class {{Name}} : System.Attribute { }

#endif
""";
}
