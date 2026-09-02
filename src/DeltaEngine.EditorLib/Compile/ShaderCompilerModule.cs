internal sealed class ShaderCompilerModule
{
    public void CompileAndImportShaders(string directory) => ShaderImportWorkflow.Run(directory);
}
