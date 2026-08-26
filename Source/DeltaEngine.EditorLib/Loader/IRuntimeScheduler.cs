using System;

namespace Delta.Engine.EditorLib.Loader;

public interface IRuntimeScheduler
{
    public event EventHandler? OnLoop;
    public void Init();
}
