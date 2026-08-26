using Avalonia.Threading;
using DeltaEngine.EditorLib.Loader;
using System;
using System.Threading.Tasks;

namespace DeltaEngine.Editor;

internal sealed class AvaloniaThreadGetter : IThreadGetter
{
    private Func<Action, Task>? _thread;
    public Func<Action, Task>? Thread => _thread ??= static x => Dispatcher.UIThread.InvokeAsync(x, DispatcherPriority.Input).GetTask();
}
