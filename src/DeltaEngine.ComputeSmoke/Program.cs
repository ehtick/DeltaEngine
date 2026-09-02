using Delta.Shader.Contract;

namespace Delta.Engine.ComputeSmoke;

[Obsolete("The legacy device smoke is retired. Use the current Delta.Render RenderGraph sample with a generated Delta.Shader.Contract.ShaderArtifact.")]
internal static class Program
{
    private static readonly ShaderAbi RetiredFixtureAbi = new(
        ShaderStage.Compute,
        workgroupSize: new ShaderWorkgroupSize(1, 1, 1));

    private static int Main()
    {
        _ = RetiredFixtureAbi;
        Console.Error.WriteLine("DeltaEngine.ComputeSmoke is retired; use a generated ShaderArtifact with Delta.Render RenderGraph.");
        return 2;
    }
}
