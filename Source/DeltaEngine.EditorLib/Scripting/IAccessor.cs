using System;
using System.Collections.Frozen;

namespace Delta.Engine.EditorLib.Scripting;

[Obsolete("The pointer-based legacy accessor contract is migration-only; use neutral component accessors in DeltaEditor.", false)]
public interface IAccessor
{
    public Type GetFieldType(string name);
    public object GetFieldValue(ref readonly object obj, string name);
    public nint GetFieldPtr(nint address, string name);
    public ReadOnlySpan<string> FieldNames { get; }
}

[Obsolete("The pointer-based legacy accessor contract is migration-only; use neutral component accessors in DeltaEditor.", false)]
public interface IAccessorsContainer
{
    public FrozenDictionary<Type, IAccessor> AllAccessors { get; }
}
