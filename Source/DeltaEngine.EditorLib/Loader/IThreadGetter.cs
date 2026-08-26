using System;
using System.Threading.Tasks;
using DeltaEngine.EditorLib.Loader;

namespace DeltaEngine.EditorLib.Loader;

public interface IThreadGetter
{
    public Func<Action, Task>? Thread { get; }
}
