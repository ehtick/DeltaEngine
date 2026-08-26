using DeltaEngine.Generation.Core;

namespace DeltaEngine.Generation.Attributes;

internal class SystemAttribute : AttributeTemplate
{
    public override string Name => nameof(SystemAttribute);

    public override string ToString() =>
$$"""
#if {{Constants.GenerateAttributes}}

using System;

namespace DeltaEngine;

[System.AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class {{Name}} : System.Attribute { }

#endif
""";
}
