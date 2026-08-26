using Avalonia;
using DeltaEngine.Runtime;
using DeltaEngine.EditorLib.Loader;
using System;

namespace DeltaEngine.Editor;

internal static class Program
{
    private static IProjectPath? _projectPath;
    private static RuntimeLoader? _runtimeLoader;

    public static IProjectPath ProjectPath => _projectPath ??
        throw new InvalidOperationException("Project path is not initialized before the editor application starts.");
    public static RuntimeLoader RuntimeLoader => _runtimeLoader ??
        throw new InvalidOperationException("Runtime loader is not initialized before the editor application starts.");

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        string directoryPath = ProjectCreator.GetExecutableDirectory();
        _projectPath = new EditorPaths(directoryPath);
        ProjectCreator.CreateProject(ProjectPath);
        IThreadGetter uiThreadGetter = new AvaloniaThreadGetter();
        _runtimeLoader = new RuntimeLoader(ProjectPath, uiThreadGetter);

        BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
    {
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.SvgImageExtension).Assembly);
        GC.KeepAlive(typeof(Avalonia.Svg.Skia.Svg).Assembly);

        return AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }
}
