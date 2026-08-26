using DeltaEngine.Runtime;
using System;
using System.IO;
using System.Text.Json;

namespace DeltaEngine.Assets.Defaults;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "The generic importer keeps the asset type in the call-site API and has no instance state.")]
public static class DefaultsImporter<T> where T : class, IAsset
{
    public static void Import(string directory)
    {
        foreach (var item in Directory.EnumerateFiles(directory))
        {
            try
            {
                var mesh = IRuntimeContext.Current.AssetImporter.LoadAsset<T>(item);
                var name = Path.ChangeExtension(Path.GetFileNameWithoutExtension(item), "mesh");
                IRuntimeContext.Current.AssetImporter.CreateAsset(mesh, name);
            }
            catch (Exception exception) when (exception is IOException or JsonException or InvalidOperationException)
            {
                // A single malformed asset must not prevent the remaining defaults from importing.
            }
        }
    }
}
