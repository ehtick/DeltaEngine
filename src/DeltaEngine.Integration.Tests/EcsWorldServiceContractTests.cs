using Delta.ECS;
using Delta.ECS.Integration;
using Xunit;

namespace Delta.Engine.Integration.Tests;

public sealed class EcsWorldServiceContractTests
{
    [Fact]
    public void SchedulesSystemsWithoutCallingWorldUpdateAndDisposesLifecycleOnce()
    {
        var world = new FakeWorld();
        var system = new RecordingSystem();
        using (var service = new EcsWorldService(world, [system]))
        {
            service.Initialize();
            service.Update(new EngineFrameContext(1, 0.016f, default));
            service.Shutdown();
            service.Shutdown();
        }

        Assert.Equal(1, world.InitializeCount);
        Assert.Equal(0, world.UpdateCount);
        Assert.Equal(1, world.ShutdownCount);
        Assert.Equal(1, system.Count);
    }

    private sealed class RecordingSystem : IEngineEcsSystem
    {
        public int Count { get; private set; }
        public void Update(IEcsWorld world, in EngineFrameContext context) => Count++;
    }

    private sealed class FakeWorld : IEcsWorld
    {
        public int InitializeCount { get; private set; }
        public int UpdateCount { get; private set; }
        public int ShutdownCount { get; private set; }
        public Stamp Stamp => default;
        public ComponentCatalog Catalog => new(ReadOnlyMemory<ComponentDescriptor>.Empty, default);
        public void Initialize() => InitializeCount++;
        public void Update() => UpdateCount++;
        public void Shutdown() => ShutdownCount++;
        public bool IsAlive(Entity entity) => false;
        public Entity Create(ReadOnlySpan<ComponentId> components) => Entity.Null;
        public bool Destroy(Entity entity) => false;
        public bool Add(Entity entity, ReadOnlySpan<ComponentId> components) => false;
        public bool Remove(Entity entity, ReadOnlySpan<ComponentId> components) => false;
        public bool TryGetComponents(Entity entity, Span<ComponentId> destination, out int totalCount) { totalCount = 0; return false; }
        public bool TryRead(Entity entity, ComponentId component, out ComponentSnapshot snapshot, out EcsReadError error) { snapshot = default; error = new(EcsReadErrorCode.Unsupported); return false; }
        public bool TryWrite(Entity entity, ComponentId component, object? value, Stamp expectedStamp, out Stamp writtenStamp, out EcsWriteError error) { writtenStamp = default; error = new(EcsWriteErrorCode.Unsupported); return false; }
    }
}
