using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Delta.Engine.Utilities;

public static class AttributeCache
{
    private static readonly ConditionalWeakTable<Type, Dictionary<Type, object?>> _typeToAttributesCache = [];
    private static Dictionary<Type, object?> GetDictionaryOfAttributes(Type type) => _typeToAttributesCache.GetOrCreateValue(type);
    public static TAttribute? GetAttribute<TAttribute>(this Type type) where TAttribute : Attribute
    {
        var attributeType = typeof(TAttribute);
        var dictionary = GetDictionaryOfAttributes(type);
        if (!dictionary.TryGetValue(attributeType, out var attribute))
        {
            dictionary[attributeType] = attribute = AttributeGetter<TAttribute>(type);
        }

        return attribute as TAttribute;
    }
    public static TAttribute? GetAttribute<TAttribute, T>() where TAttribute : Attribute => typeof(T).GetAttribute<TAttribute>();
    public static bool HasAttribute<TAttribute, T>() where TAttribute : Attribute => typeof(T).GetAttribute<TAttribute>() != null;
    public static bool HasAttribute<TAttribute>(this Type type) where TAttribute : Attribute => type.GetAttribute<TAttribute>() != null;

    private static TAttribute? AttributeGetter<TAttribute>(Type objectType) where TAttribute : Attribute => objectType.GetCustomAttribute<TAttribute>(false);
}
