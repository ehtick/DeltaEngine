using Arch.Core;
namespace Delta.Engine.ECS;

[System.Obsolete("Arch EntityReference helpers are migration-only; use EngineEntityId and IEcsWorld.", false)]
public static class EntityReferenceExtensions
{
    public static bool IsAlive(this EntityReference entityRef)
    {
        return entityRef.IsAlive();
    }
}
