internal static class ShaderImportWorkflow
{
    public static void Run(string directory)
    {
        foreach (var module in ShaderModuleCatalog.Read(directory))
        {
            if (ShaderModuleCatalog.IsGraphicsModule(module.Value))
            {
                ShaderAssetWriter.Write(module.Key, module.Value);
            }
        }
    }
}
