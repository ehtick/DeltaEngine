namespace Delta.Engine.Assets.Defaults;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1000:Do not declare static members on generic types",
    Justification = "The generic importer keeps the asset type in the call-site API and has no instance state.")]
public static class DefaultsImporter<T> where T : class, IAsset
{
    public static void Import(string directory) => DefaultsAssetImportRunner.Import<T>(directory);
}
