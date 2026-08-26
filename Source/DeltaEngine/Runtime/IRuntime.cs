using System;
namespace DeltaEngine.Runtime;

public interface IRuntime : IDisposable
{
    public IRuntimeContext Context { get; }
    public void Run();
}
