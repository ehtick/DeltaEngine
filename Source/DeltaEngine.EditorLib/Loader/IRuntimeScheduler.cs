using System;

namespace DeltaEngine.EditorLib.Loader;

public interface IRuntimeScheduler
{
    public event EventHandler? OnLoop;
    public void Init();
}
