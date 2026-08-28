using System.Text.Json;
using Delta.Shader.Abstractions;

namespace Delta.Engine.ComputeSmoke;

internal static class GeneratedShaderArtifactLoader
{
    public static async Task<ShaderArtifact> LoadAsync(
        string spirvPath,
        string manifestPath,
        CancellationToken cancellationToken = default)
    {
        var spirv = await File.ReadAllBytesAsync(spirvPath, cancellationToken).ConfigureAwait(false);
        var manifestJson = await File.ReadAllTextAsync(manifestPath, cancellationToken).ConfigureAwait(false);
        var manifest = JsonSerializer.Deserialize<ShaderAbiManifest>(manifestJson)
            ?? throw new InvalidDataException($"Shader ABI manifest is empty: {manifestPath}");

        return new ShaderArtifact(spirv, manifest);
    }
}
