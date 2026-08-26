using System;

namespace DeltaEngine.Scripting;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EditableAttribute() : Attribute;
