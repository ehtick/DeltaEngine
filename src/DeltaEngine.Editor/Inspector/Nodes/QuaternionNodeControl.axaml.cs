#pragma warning disable CS8618 // Avalonia initializes XAML-bound controls during InitializeComponent.
using Arch.Core;
using Avalonia.Controls;
using Avalonia.Media;
using Delta.Engine.Runtime;
using Delta.Engine.Editor.Inspector.Internal;
using ExCSS;
using Delta;
using System;

namespace Delta.Engine.Editor;

internal sealed partial class QuaternionNodeControl : InspectorNode
{
    private readonly NodeData _nodeData;
    public QuaternionNodeControl() => InitializeComponent();
    public QuaternionNodeControl(NodeData nodeData) : this()
    {
        FieldName.Content = (_nodeData = nodeData).FieldName;
        FieldX.OnDrag += x => _nodeData.DragFloat(FieldX.FieldData, x, 1);
        FieldY.OnDrag += x => _nodeData.DragFloat(FieldY.FieldData, x, 1);
        FieldZ.OnDrag += x => _nodeData.DragFloat(FieldZ.FieldData, x, 1);
    }

    public override void SetLabelColor(IBrush brush) => FieldName.Foreground = brush;

    public override bool UpdateData(ref EntityReference entity)
    {
        if (!ClipVisible)
        {
            return false;
        }

        var euler = Degrees(_nodeData.GetData<quaternion>(ref entity));

        bool changed = SetField(FieldX.FieldData, ref euler.x) |
                       SetField(FieldY.FieldData, ref euler.y) |
                       SetField(FieldZ.FieldData, ref euler.z);
        if (changed)
        {
            _nodeData.SetData(ref entity, ToQuaternion(euler));
        }

        return changed;
    }

    private static bool SetField(TextBox field, ref float angle)
    {
        bool changed = field.IsFocused;
        if (!changed)
        {
            field.Text = angle.ParseToString();
        }
        else if (field.Text.ParseToFloat(out var parsed))
        {
            angle = parsed;
        }

        return changed;
    }


    public static quaternion ToQuaternion(float3 v)
    {
        float pi = Maths.Radians(180f);
        v /= 360f;
        v *= pi;
        float sx = Maths.Sin(v.x);
        float cx = Maths.Cos(v.x);
        float sy = Maths.Sin(v.y);
        float cy = Maths.Cos(v.y);
        float sz = Maths.Sin(v.z);
        float cz = Maths.Cos(v.z);
        float cysz = cy * sz;
        float cycz = cy * cz;
        float sycz = sy * cz;
        float sysz = sy * sz;
        return new quaternion(
            -(cx * sysz) + (sx * cycz),
            (cx * sycz) + (sx * cysz),
            (cx * cysz) - (sx * sycz),
            (cx * cycz) + (sx * sysz));
    }

    public static float3 Degrees(quaternion q)
    {
        var qY2 = q.y * q.y;
        float sinr_cosp = 2 * (q.w * q.x + q.y * q.z);
        float siny_cosp = 2 * (q.w * q.z + q.x * q.y);
        float cosr_cosp = 1 - (2 * (q.x * q.x + qY2));
        float cosy_cosp = 1 - (2 * (qY2 + q.z * q.z));
        float sinp = 2 * (q.w * q.y - q.z * q.x);
        float pi = Maths.Radians(180f);
        float toDegrees = 180f / pi;
        return new float3(
            Maths.Atan2(sinr_cosp, cosr_cosp) * toDegrees,
            (Maths.Abs(sinp) >= 1 ? (sinp < 0 ? -pi / 2 : pi / 2) : Maths.Asin(sinp)) * toDegrees,
            Maths.Atan2(siny_cosp, cosy_cosp) * toDegrees);
    }

}
