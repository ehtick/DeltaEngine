using System;

namespace DeltaEngine.Runtime;

internal sealed class SceneManager : ISceneManager
{
    private Scene _scene;

    public Scene CurrentScene
    {
        get => _scene;
        private set
        {
            _scene = value;
            OnSceneChanged?.Invoke(this, new SceneChangedEventArgs(value));
        }
    }

    public event EventHandler<SceneChangedEventArgs>? OnSceneChanged;

    public SceneManager()
    {
        _scene = new Scene();
    }

    public void LoadScene(string path)
    {
    }

    public void SaveScene(string name)
    {
        if (_scene != null)
        {
            IRuntimeContext.Current.AssetImporter.CreateAsset(_scene, name);
        }
    }

    public void CreateScene()
    {
    }

    public void CreateTestScene()
    {
        return;
    }

    public void Dispose() => _scene.Dispose();
}
