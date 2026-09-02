namespace Delta.Engine.Utilities;

internal static class Enums
{
    public static int GetCount<T>() where T : struct, Enum => EnumBakedValues<T>.count;
    public static ReadOnlySpan<T> GetValues<T>() where T : struct, Enum => EnumBakedValues<T>.values;

    /// <summary>
    /// Returns string representing current enum or enum flags.
    /// All elements are distinct by its undrelying type e.g. int, long, uint.
    /// If element is marked as obsolete, it's name will not be used
    /// except cases when replacement not found
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="value"></param>
    /// <returns></returns>
    public static string ToString<T>(T value) where T : unmanaged, Enum
        => EnumFormatter<T>.Format(value);
}
