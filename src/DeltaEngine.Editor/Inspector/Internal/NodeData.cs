using Arch.Core;
using Avalonia.Controls;
using Delta.Engine.EditorLib.Scripting;
using Delta.Maths;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Delta.Engine.Editor.Inspector.Internal;

internal sealed class NodeData(RootData root, PathData path)
{
    private readonly RootData _rootData = root;
    private readonly PathData _pathData = path;

    public NodeData(RootData root) : this(root, new([])) { }

    public Type Component => _rootData.Component;
    public string FieldName => Path.Length == 0 ? _rootData.componentName : Path[^1];
    public IAccessorsContainer Accessors => _rootData.Accessors;
    public Type FieldType => _rootData.Accessors.GetFieldType(_rootData.Component, Path);
    public ReadOnlySpan<string> Path => _pathData.Path;
    public ReadOnlySpan<string> FieldNames => _rootData.Accessors.AllAccessors[_rootData.Accessors.GetFieldType(_rootData.Component, Path)].FieldNames;
    public NodeData ChildData(string fieldName) => new(_rootData, new([.. Path, fieldName]));
    public T GetData<T>(ref EntityReference entity) => _rootData.Accessors.GetComponentFieldValue<T>(entity, _rootData.Component, Path);
    public void SetData<T>(ref EntityReference entity, T data) => _rootData.Accessors.SetComponentFieldValue(entity, _rootData.Component, Path, data);


    public bool UpdateFloat(TextBox fieldData, ref EntityReference entity)
    {
        ArgumentNullException.ThrowIfNull(fieldData);
        bool changed = fieldData.IsFocused;
        if (!changed)
        {
            fieldData.Text = GetData<float>(ref entity).ParseToString();
        }
        else if (fieldData.Text.ParseToFloat(out var value))
        {
            SetData(ref entity, value);
        }

        return changed;
    }

    public void DragFloat(TextBox fieldData, float delta, float multiplier)
    {
        ArgumentNullException.ThrowIfNull(fieldData);
        if (!fieldData.IsFocused)
        {
            fieldData.Focus();
        }

        if (fieldData.Text.ParseToFloat(out var value))
        {
            value += delta * multiplier;
        }

        fieldData.Text = value.ParseToStringHighRes();
    }

    public void DragInt(TextBox fieldData, float delta)
    {
        ArgumentNullException.ThrowIfNull(fieldData);
        if (!fieldData.IsFocused)
        {
            fieldData.Focus();
        }

        if (int.TryParse(fieldData.Text, out int value))
        {
            value += Maths.Sign(delta);
        }

        fieldData.Text = value.ParseToString();
    }

    public bool UpdateString(TextBox FieldData, ref EntityReference entity)
    {
        ArgumentNullException.ThrowIfNull(FieldData);
        bool changed = FieldData.IsFocused;
        if (!changed)
        {
            FieldData.Text = GetData<string>(ref entity);
        }
        else if (FieldData.Text != null)
        {
            SetData(ref entity, FieldData.Text);
        }

        return changed;
    }

    public bool UpdateInt(TextBox fieldData, ref EntityReference entity)
    {
        ArgumentNullException.ThrowIfNull(fieldData);
        bool changed = fieldData.IsFocused;
        if (!changed)
        {
            fieldData.Text = GetData<int>(ref entity).ParseToString();
        }
        else
        {
            if (string.IsNullOrEmpty(fieldData.Text))
            {
                SetData(ref entity, default(int));
            }
            else if (int.TryParse(fieldData.Text, out int result))
            {
                SetData(ref entity, result);
            }
        }
        return changed;
    }
}

internal sealed record RootData(Type Component, IAccessorsContainer Accessors)
{
    public readonly string componentName = Component.Name;
    public IAccessorsContainer Accessors = Accessors;
}
internal sealed class PathData
{
    private readonly List<string> _path;

    public PathData(List<string> path)
    {
        ArgumentNullException.ThrowIfNull(path);
        _path = path;
    }

    public ReadOnlySpan<string> Path => CollectionsMarshal.AsSpan(_path);
}
