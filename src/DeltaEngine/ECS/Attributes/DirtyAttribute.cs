using System;

namespace Delta.Engine.ECS.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
public sealed class DirtyAttribute : Attribute { }
