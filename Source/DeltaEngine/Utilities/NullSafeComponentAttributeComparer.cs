using Arch.Core.Utils;
using System;
using System.Collections.Generic;
namespace Delta.Engine.Utilities;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Default exposes a cached comparer instance for the generic attribute type.")]
public sealed class NullSafeComponentAttributeComparer<TAttribute> : IComparer<ComponentType> where TAttribute : Attribute
{
    private static readonly NullSafeComponentAttributeComparer<TAttribute> _defaultComparer = new();
    public static NullSafeComponentAttributeComparer<TAttribute> Default => _defaultComparer;
    public int Compare(ComponentType x, ComponentType y)
    {
        var attr1 = x.Type.GetAttribute<TAttribute>();
        var attr2 = y.Type.GetAttribute<TAttribute>();
        return NullSafeComparer<TAttribute>.Default.Compare(attr2, attr1);
    }
}
