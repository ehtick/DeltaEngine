using System;

namespace Delta.Engine.EditorLib.Loader;

[Obsolete("The legacy editor scheduler is migration-only; use explicit EngineHost frame stages.", false)]
public interface IRuntimeScheduler
{
    public event EventHandler? OnLoop;
    public void Init();
}
