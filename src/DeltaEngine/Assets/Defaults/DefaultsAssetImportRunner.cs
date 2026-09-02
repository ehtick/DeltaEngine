using System.IO;

namespace Delta.Engine.Assets.Defaults;

internal static class DefaultsAssetImportRunner
{
    public static void Import<T>(string directory) where T : class, IAsset
    {
        foreach (var item in Directory.EnumerateFiles(directory))
        {
            DefaultsAssetFileImporter.TryImport<T>(item);
        }
    }
}
