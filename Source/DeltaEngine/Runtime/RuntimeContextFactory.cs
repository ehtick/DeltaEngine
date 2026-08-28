using Delta.Engine.Rendering;
namespace Delta.Engine.Runtime;

[System.Obsolete("The legacy runtime context factory is migration-only; use explicit EngineHost composition.", false)]
public static class RuntimeContextFactory
{
    public static IRuntimeContext CreateHeadlessContext(IProjectPath projectPath)
        => CreateContext(projectPath);

    public static IRuntimeContext CreateWindowedContext(IProjectPath projectPath)
        => CreateContext(projectPath);

    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000:Dispose objects before losing scope",
        Justification = "The returned DefaultRuntimeContext owns and disposes the scene manager and graphics module.")]
    private static DefaultRuntimeContext CreateContext(IProjectPath projectPath)
    {
        var assets = new GlobalAssetCollection();
        var sceneManager = new SceneManager();
        var graphics = new NullGraphicsModule("Delta Editor");

        return new DefaultRuntimeContext(projectPath, assets, sceneManager, graphics);
    }
}
