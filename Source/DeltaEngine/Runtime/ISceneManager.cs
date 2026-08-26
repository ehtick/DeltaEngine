using System;
namespace DeltaEngine.Runtime;

public sealed class SceneChangedEventArgs(Scene scene) : EventArgs
{
    public Scene Scene { get; } = scene;
}

public interface ISceneManager : IDisposable
{
    public event EventHandler<SceneChangedEventArgs>? OnSceneChanged;
    public Scene CurrentScene { get; }
    public void LoadScene(string path);
    public void SaveScene(string name);
}
