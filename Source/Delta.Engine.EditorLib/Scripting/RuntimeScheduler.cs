using Delta.Engine.Runtime;
using Delta.Engine.EditorLib.Loader;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Delta.Engine.EditorLib.Scripting;

internal sealed class RuntimeScheduler : IRuntimeScheduler, IDisposable
{
    private readonly IRuntime _runtime;
    private readonly IThreadGetter? _uiThreadGetter;
    private Thread? _runtimeThread;
    private bool _disposed;

    public event EventHandler? OnLoop;

    public RuntimeScheduler(IRuntime runtime, IThreadGetter? uiThreadGetter)
    {
        _runtime = runtime;
        _uiThreadGetter = uiThreadGetter;
    }

    public void Init()
    {
        _runtimeThread = new Thread(Loop);
        _runtimeThread.Name = "RuntimeThread." + _runtimeThread.ManagedThreadId;
        _runtimeThread.Start();
    }

    private void Loop()
    {
        while (!_disposed)
        {
            if (_uiThreadGetter != null && _uiThreadGetter.Thread != null)
            {
                _uiThreadGetter.Thread(Execute).Wait();
            }
            else
            {
                Execute();
            }
        }
    }

    private void Execute()
    {
        OnLoop?.Invoke(this, EventArgs.Empty);
        _runtime.Run();
    }

    public void Dispose()
    {
        _disposed = true;
        _runtimeThread = null;
    }
}
