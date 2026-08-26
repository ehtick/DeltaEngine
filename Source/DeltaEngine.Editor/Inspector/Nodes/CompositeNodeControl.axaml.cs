using Arch.Core;
using Avalonia.Controls;
using Avalonia.Media;
using DeltaEngine.Runtime;
using DeltaEngine.Editor.Hierarchy;
using DeltaEngine.Editor.Inspector;
using DeltaEngine.Editor.Inspector.Internal;

namespace DeltaEngine.Editor;

internal sealed partial class CompositeNodeControl : InspectorNode
{
    private IListWrapper<InspectorNode, Control> ChildrenNodes => new(ChildrenStack.Children);
    public CompositeNodeControl() => InitializeComponent();

    public CompositeNodeControl(NodeData nodeData) : this()
    {
        FieldName.Content = nodeData.FieldName;
        int fieldsCount = nodeData.FieldNames.Length;
        for (int i = 0; i < fieldsCount; i++)
        {
            var childNodeData = nodeData.ChildData(nodeData.FieldNames[i]);
            ChildrenNodes.Add(NodeFactory.CreateNode(childNodeData));
        }
    }

    public override bool UpdateData(ref EntityReference entity)
    {
        if (!ClipVisible)
        {
            return false;
        }

        bool changed = false;
        for (int i = 0; i < ChildrenNodes.Count; i++)
        {
            changed |= ChildrenNodes[i].UpdateData(ref entity);
        }

        return changed;
    }

    public override void SetLabelColor(IBrush brush) => FieldName.Foreground = brush;
}
