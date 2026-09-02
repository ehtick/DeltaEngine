using Delta.Engine.Runtime;
using System;
using System.IO;
using System.Text.Json;

namespace Delta.Engine.Assets.Defaults;

internal static class DefaultsAssetFileImporter
{
    public static void TryImport<T>(string path) where T : class, IAsset
    {
        try
        {
            var mesh = IRuntimeContext.Current.AssetImporter.LoadAsset<T>(path);
            var name = Path.ChangeExtension(Path.GetFileNameWithoutExtension(path), "mesh");
            IRuntimeContext.Current.AssetImporter.CreateAsset(mesh, name);
        }
        catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException)
        {
            // A single malformed asset must not prevent the remaining defaults from importing.
        }
    }
}
