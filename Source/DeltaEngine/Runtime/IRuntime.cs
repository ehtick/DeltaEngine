using System;
namespace Delta.Engine.Runtime;

[Obsolete("The Arch-backed IRuntime is migration-only; use IEngineHost.", false)]
public interface IRuntime : IDisposable
{
    public IRuntimeContext Context { get; }
    public void Run();
}
