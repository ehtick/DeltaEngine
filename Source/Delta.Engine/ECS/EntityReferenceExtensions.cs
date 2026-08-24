using Arch.Core;
namespace Delta.Engine.ECS;

public static class EntityReferenceExtensions
{
    public static bool IsAlive(this EntityReference entityRef)
    {
        return entityRef.IsAlive();
    }
}
