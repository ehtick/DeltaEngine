using Avalonia.Controls;
using Avalonia.Interactivity;
using DeltaEngine.Runtime;
using System.Diagnostics;
using System.IO;
using System;

namespace DeltaEngine.Editor;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        if (Design.IsDesignMode)
        {
            return;
        }

        //Program.RuntimeLoader
        GC.KeepAlive(new FlyoutSearchControl());
        Program.RuntimeLoader.OnLoop += (_, _) => Inspector.UpdateInspector();
        Program.RuntimeLoader.OnLoop += (_, _) => Hierarchy.UpdateHierarchy();
        Program.RuntimeLoader.OnLoop += (_, _) => Scene.UpdateScene();

        Hierarchy.OnEntitySelected += (_, e) => Inspector.SetSelectedEntity(e.Entity);
        Program.RuntimeLoader.Init();
    }

    private void CreateTestScene(object? sender, RoutedEventArgs e) { }

    private void OpenProjectFolder(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Directory.Exists(IRuntimeContext.Current.ProjectPath.RootDirectory))
            {
                Process.Start("explorer.exe", IRuntimeContext.Current.ProjectPath.RootDirectory);
            }
        }
        catch (Exception exception) when (exception is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
        }
    }

    private void OpenTempFolder(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (Directory.Exists(IRuntimeContext.Current.ProjectPath.TempDirectory))
            {
                Process.Start("explorer.exe", IRuntimeContext.Current.ProjectPath.TempDirectory);
            }
        }
        catch (Exception exception) when (exception is IOException or System.ComponentModel.Win32Exception or InvalidOperationException)
        {
        }
    }
}
