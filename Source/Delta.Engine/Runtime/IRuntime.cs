using System;
namespace Delta.Engine.Runtime;

public interface IRuntime : IDisposable
{
    public IRuntimeContext Context { get; }
    public void Run();
}
