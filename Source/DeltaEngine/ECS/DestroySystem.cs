using Arch.Core;
using Delta.Engine.ECS.Components;
using Delta.Engine.Runtime;
internal static class DestroySystem
{
    private static readonly QueryDescription _destroyDescription = new QueryDescription().WithAll<DestroyFlag>();
    public static void Execute()
    {
        IRuntimeContext.Current.SceneManager.CurrentScene._world.Destroy(_destroyDescription);
    }
}
