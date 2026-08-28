using System;
namespace Delta.Engine.Runtime;

[System.Obsolete("The Arch-backed scene manager is migration-only; use explicit EngineHost and hierarchy adapters.", false)]
public sealed class SceneChangedEventArgs(Scene scene) : EventArgs
{
    public Scene Scene { get; } = scene;
}

[System.Obsolete("The Arch-backed scene manager is migration-only; use explicit EngineHost and EcsWorldService.", false)]
public interface ISceneManager : IDisposable
{
    public event EventHandler<SceneChangedEventArgs>? OnSceneChanged;
    public Scene CurrentScene { get; }
    public void LoadScene(string path);
    public void SaveScene(string name);
}
