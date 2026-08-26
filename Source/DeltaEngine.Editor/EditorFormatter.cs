using Arch.Core;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace DeltaEngine.Editor;

internal static class EditorFormatter
{
    private const string FloatFormat = "0.00";
    private static readonly Dictionary<EntityReference, string> _lookupEntityReference = [];
    private static readonly CultureInfo _editorCulture = CreateEditorCulture();

    private static CultureInfo CreateEditorCulture()
    {
        var culture = (CultureInfo)CultureInfo.InvariantCulture.Clone();
        culture.NumberFormat = (NumberFormatInfo)culture.NumberFormat.Clone();
        culture.NumberFormat.NumberDecimalSeparator = ".";
        return culture;
    }

    public static string ParseToString(this float value) => value.ToString(FloatFormat, _editorCulture);
    public static string ParseToStringHighRes(this float value) => value.ToString("F", _editorCulture);

    public static bool ParseToFloat(this string? value, out float parsed)
    {
        parsed = default;
        if (string.IsNullOrEmpty(value))
        {
            return true;
        }
        else if (float.TryParse(value, NumberStyles.Float, _editorCulture, out parsed))
        {
            return true;
        }

        return false;
    }

    public static string ParseToString(this int value) => value.ToString(CultureInfo.InvariantCulture);

    public static string LookupString(this EntityReference value)
    {
        if (!_lookupEntityReference.TryGetValue(value, out var result))
        {
            _lookupEntityReference[value] = result = $"id: {value.Entity.Id}, ver: {value.Version}";
        }

        return result;
    }
}
