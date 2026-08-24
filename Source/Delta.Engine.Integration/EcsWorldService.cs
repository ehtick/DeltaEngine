using Delta.ECS.Integration;

namespace Delta.Engine.Integration;

public interface IEngineEcsSystem
{
    void Update(IEcsWorld world, in EngineFrameContext context);
}

public sealed class EcsWorldService : IEngineWorldService
{
    private readonly IEcsWorld _world;
    private readonly IReadOnlyList<IEngineEcsSystem> _systems;
    private readonly bool _disposeWorld;
    private bool _initialized;
    private bool _shutdown;
    private bool _disposed;

    public EcsWorldService(IEcsWorld world, IEnumerable<IEngineEcsSystem>? systems = null, bool disposeWorld = false)
    {
        ArgumentNullException.ThrowIfNull(world);
        _world = world;
        _systems = systems is null ? Array.Empty<IEngineEcsSystem>() : systems.ToArray();
        _disposeWorld = disposeWorld;
    }

    public void Initialize()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialized)
        {
            return;
        }

        _world.Initialize();
        _initialized = true;
    }

    public void Update(in EngineFrameContext context)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_initialized || _shutdown)
        {
            throw new InvalidOperationException("The ECS world service is not running.");
        }

        foreach (var system in _systems)
        {
            system.Update(_world, in context);
        }
    }

    public void Shutdown()
    {
        if (_shutdown || !_initialized)
        {
            return;
        }

        _world.Shutdown();
        _shutdown = true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Shutdown();
        if (_disposeWorld && _world is IDisposable disposable)
        {
            disposable.Dispose();
        }

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}
