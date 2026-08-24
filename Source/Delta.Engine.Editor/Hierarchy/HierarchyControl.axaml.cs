using Arch.Core;
using Arch.Core.Extensions;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Delta.Engine.ECS;
using Delta.Engine.ECS.Components;
using Delta.Engine.Runtime;
using Delta.Engine.Editor.Hierarchy;
using System;
using System.Collections.Generic;

namespace Delta.Engine.Editor;

public partial class HierarchyControl : UserControl
{
    public event EventHandler<EntityReferenceEventArgs>? OnEntitySelected;
    private readonly HierarchyNodeCreator _hierarchyNodeCreator = new();

    private IListWrapper<HierarchyNodeControl, Control> ChildrenNodes => new(EntityNodeStack.Children);

    public HierarchyControl()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        _hierarchyNodeCreator.OnEntityRemoveRequest += (_, e) => RemoveEntity(e.Entity);
        _hierarchyNodeCreator.OnEntitySelectRequest += (_, e) => SelectEntity(e.Entity);
    }

    public void UpdateHierarchy()
    {
        PanelHeader.StartDebug();

        var entities = IRuntimeContext.Current.SceneManager.CurrentScene.GetRootEntities();
        int count = entities.Length;

        UpdateChildrenCount(count);

        for (int i = 0; i < count; i++)
        {
            ChildrenNodes[i].UpdateEntity(entities[i]);
        }

        PanelHeader.StopDebug();
    }


    private void CreateNewEntity(object? sender, RoutedEventArgs e)
    {
        SelectEntity(IRuntimeContext.Current.SceneManager.CurrentScene.AddEntity());
    }

    private void RemoveEntity(EntityReference entity)
    {
        IRuntimeContext.Current.SceneManager.CurrentScene.RemoveEntity(entity);
    }

    private void UpdateChildrenCount(int neededNodesCount)
    {
        var currentNodesCount = ChildrenNodes.Count;
        var delta = currentNodesCount - neededNodesCount;
        if (delta > 0)
        {
            for (int i = 0; i < delta; i++)
            {
                ChildrenNodes[^1].Dispose();
                ChildrenNodes.RemoveAt(ChildrenNodes.Count - 1);
            }
        }
        else if (delta < 0)
        {
            for (int i = delta; i < 0; i++)
            {
                var node = _hierarchyNodeCreator.GetOrCreateNode();
                ChildrenNodes.Add(node);
            }
        }
    }

    private void SelectEntity(EntityReference entityRef)
    {
        OnEntitySelected?.Invoke(this, new EntityReferenceEventArgs(entityRef));
    }

    private void Deselect()
    {
        OnEntitySelected?.Invoke(this, new EntityReferenceEventArgs(EntityReference.Null));
    }
}
