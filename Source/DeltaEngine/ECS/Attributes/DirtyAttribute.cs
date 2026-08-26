using System;

namespace DeltaEngine.ECS.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class DirtyAttribute : Attribute { }
