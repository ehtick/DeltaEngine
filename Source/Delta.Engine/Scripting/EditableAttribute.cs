using System;

namespace Delta.Engine.Scripting;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
public sealed class EditableAttribute() : Attribute;
