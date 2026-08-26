using Delta.Engine.Runtime;
using System;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Delta.Engine.Assets;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1040:Avoid empty interfaces",
    Justification = "IAsset is the intentional marker boundary shared by runtime asset handles and editor importers.")]
public interface IAsset { }

[DebuggerDisplay("{ToString(),nq}")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification = "The public guid field is the established serialized asset-handle representation.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "Generic asset handles expose conversion operators and a named conversion helper as their value API.")]
[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Usage",
    "CA2225:Operator overloads have named alternates",
    Justification = "The generic implicit conversion cannot use a stable concrete return-type name; ToAsset is the named alternative.")]
public readonly struct GuidAsset<T> : IEquatable<GuidAsset<T>>, IComparable<GuidAsset<T>> where T : class, IAsset
{
    private const string NullDataString = "null";
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1051:Do not declare visible instance fields",
        Justification = "The public guid field is the established serialized asset-handle representation.")]
    public readonly Guid guid;

    [JsonConstructor]
    internal GuidAsset(Guid guid) => this.guid = guid;

    public readonly T GetAsset() => IRuntimeContext.Current.AssetImporter.GetAsset(this);
    public readonly string GetAssetNameOrDefault() => Null ? NullDataString : IRuntimeContext.Current.AssetImporter.GetName(this);

    public override string ToString()
    {
        if (Null)
        {
            return NullDataString;
        }

        Span<byte> guidBytes = stackalloc byte[16];
        Span<char> guidChars = stackalloc char[24];
        guid.TryWriteBytes(guidBytes);
        Convert.TryToBase64Chars(guidBytes, guidChars, out var _);
        return new string(guidChars[..22]);
    }

    [Imp(Inl)]
    public static implicit operator T(GuidAsset<T> guidAsset) => IRuntimeContext.Current.AssetImporter.GetAsset(guidAsset);

    public static T ToAsset(GuidAsset<T> guidAsset) => guidAsset;

    [Imp(Inl)]
    public readonly int CompareTo(GuidAsset<T> other) => guid.CompareTo(other.guid);

    [Imp(Inl)]
    public override readonly int GetHashCode() => guid.GetHashCode(); // TODO override hashCode? just return first 32 bits?
    [Imp(Inl)]
    public readonly bool Equals(GuidAsset<T> other) => guid.Equals(other.guid);
    [Imp(Inl)]
    public override bool Equals(object? obj) => obj is GuidAsset<T> asset && Equals(asset);

    [Imp(Inl)]
    public static bool operator ==(GuidAsset<T> left, GuidAsset<T> right) => left.Equals(right);
    [Imp(Inl)]
    public static bool operator !=(GuidAsset<T> left, GuidAsset<T> right) => !left.Equals(right);

    public static bool operator <(GuidAsset<T> left, GuidAsset<T> right) => left.CompareTo(right) < 0;

    public static bool operator <=(GuidAsset<T> left, GuidAsset<T> right) => left.CompareTo(right) <= 0;

    public static bool operator >(GuidAsset<T> left, GuidAsset<T> right) => left.CompareTo(right) > 0;

    public static bool operator >=(GuidAsset<T> left, GuidAsset<T> right) => left.CompareTo(right) >= 0;

    public bool Null => guid == Guid.Empty;
}
