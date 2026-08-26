using System;
using System.Runtime.CompilerServices;

namespace DeltaEngine.ECS.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1051:Do not declare visible instance fields",
    Justification = "Attribute fields are consumed by the source generator and are immutable metadata.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1708:Identifiers should differ by more than case",
    Justification = "The lowercase fields are part of the existing generator metadata contract.")]
public sealed class ComponentAttribute : Attribute, IComparable<ComponentAttribute>, IEquatable<ComponentAttribute>
{
    private readonly bool _builtIn;
    private readonly int _order;

    private readonly int _sourceLineNumber;
    private readonly string _sourceFilePath;

    public bool BuiltIn => _builtIn;
    public int Order => _order;
    public int SourceLineNumber => _sourceLineNumber;
    public string SourceFilePath => _sourceFilePath;

    internal ComponentAttribute(int order, bool builtIn, [CallerLineNumber] int sourceLineNumber = 0, [CallerFilePath] string sourceFilePath = "")
    {
        _order = order;
        _builtIn = builtIn;
        _sourceLineNumber = sourceLineNumber;
        _sourceFilePath = sourceFilePath;
    }

    public ComponentAttribute(int order = 0) : this(order, false) { }

    public int CompareTo(ComponentAttribute? other)
    {
        if (other == null)
        {
            return -1;
        }

        var builtInCompare = _builtIn.CompareTo(other._builtIn);
        var orderCompare = _order.CompareTo(other._order);

        return builtInCompare != 0 ? builtInCompare : orderCompare;
    }

    private static int Compare(ComponentAttribute? left, ComponentAttribute? right) => left is null
        ? right is null ? 0 : -1
        : left.CompareTo(right);

    public static bool operator <(ComponentAttribute? left, ComponentAttribute? right) => Compare(left, right) < 0;
    public static bool operator <=(ComponentAttribute? left, ComponentAttribute? right) => Compare(left, right) <= 0;
    public static bool operator >(ComponentAttribute? left, ComponentAttribute? right) => Compare(left, right) > 0;
    public static bool operator >=(ComponentAttribute? left, ComponentAttribute? right) => Compare(left, right) >= 0;

    public bool Equals(ComponentAttribute? other) => other is not null &&
        _builtIn == other._builtIn &&
        _order == other._order &&
        _sourceLineNumber == other._sourceLineNumber &&
        string.Equals(_sourceFilePath, other._sourceFilePath, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is ComponentAttribute other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(_builtIn, _order, _sourceLineNumber, _sourceFilePath);

    public static bool operator ==(ComponentAttribute? left, ComponentAttribute? right) => Equals(left, right);

    public static bool operator !=(ComponentAttribute? left, ComponentAttribute? right) => !Equals(left, right);
}
