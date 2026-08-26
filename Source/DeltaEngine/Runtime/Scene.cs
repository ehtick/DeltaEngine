using Arch.Core;
using Delta.Engine.Assets;
using Delta.Engine.ECS;
using Delta.Engine.ECS.Components;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.Json.Serialization;

namespace Delta.Engine.Runtime;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Reliability",
    "CA2213:Disposable fields should be disposed",
    Justification = "Arch.Core requires World.Destroy(World); calling World.Dispose directly is not the supported ownership path.")]
public sealed class Scene : IDisposable, IAsset
{
    internal readonly World _world;
    [JsonIgnore]
    private readonly List<ISystem> _jobs;

    private readonly List<ISystem> _defaultJobs;

    [JsonIgnore]
    private readonly HierarchySystem _hierarchySystem = new();
    private readonly Stopwatch _deltaTimeSw = new();
    private float _deltaTime;

    public Scene()
    {
        _world = World.Create();
        _jobs = [];
        _defaultJobs = [_hierarchySystem.MarkDestroySystem()];
    }

    public float DeltaTime() => _deltaTime;

    public void Run(float deltaTime)
    {
        if (IRuntimeContext.Current.Running)
        {
            _deltaTime = deltaTime;
            foreach (var item in _jobs)
            {
                item.Execute();
            }
        }
        foreach (var item in _defaultJobs)
        {
            item.Execute();
        }
    }

    public void Run()
    {
        var deltaTime = (float)_deltaTimeSw.Elapsed.TotalSeconds;
        _deltaTimeSw.Restart();
        Run(deltaTime);
    }

    [Imp(Sync)]
    public void AddSystem(ISystem job) => _jobs.Add(job);
    [Imp(Sync)]
    public void RemoveSystem(ISystem job) => _jobs.Remove(job);
    [Imp(Sync)]
    public void RemoveEntity(EntityReference entityRef)
    {
        IRuntimeContext.Current.SceneManager.CurrentScene._world.AddOrGet<DestroyFlag>(entityRef);
    }

    [Imp(Sync)]
    public EntityReference AddEntity()
    {
        var entityRef = _world.Reference(_world.Create());
        _hierarchySystem.AddRootEntity(entityRef);
        return entityRef;
    }

    public EntityReference[] GetRootEntities()
    {
        return _hierarchySystem.GetRootEntities();
    }
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Design",
        "CA1002:Do not expose generic lists",
        Justification = "Hierarchy adapters reuse a caller-owned list to avoid per-frame allocations.")]
    public void GetFirstChildren(EntityReference entityRef, List<EntityReference> children)
    {
        ArgumentNullException.ThrowIfNull(children);
        _hierarchySystem.GetFirstChildren(entityRef, children);
    }
    public int GetFirstChildrenCount(EntityReference entityRef)
    {
        return _hierarchySystem.GetFirstChildrenCount(entityRef);
    }

    public void Dispose()
    {
        World.Destroy(_world);
        foreach (var item in _jobs)
        {
            if (item is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _jobs.Clear();
    }
}
