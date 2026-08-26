using System;
using System.Linq;
using DeltaEngine.Integration;
using DeltaEditor.Scripting;
using Xunit;

namespace DeltaEngine.Integration.Tests;

public sealed class RuntimeBoundaryContractTests
{
    [Fact]
    public void IntegrationRuntimeDoesNotReferenceEditorOrBackendAssemblies()
    {
        var references = typeof(EngineHost)
            .Assembly
            .GetReferencedAssemblies()
            .Select(static reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.DoesNotContain(references, static name => name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.StartsWith("Avalonia", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.StartsWith("Arch", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.StartsWith("DeltaEngine.Editor", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.StartsWith("DeltaRender", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.StartsWith("DeltaShader", StringComparison.Ordinal));
    }

    [Fact]
    public void RoslynCompilerBackendDependsOnNeutralContractsInOneDirection()
    {
        var references = typeof(RoslynScriptCompiler)
            .Assembly
            .GetReferencedAssemblies()
            .Select(static reference => reference.Name ?? string.Empty)
            .ToArray();

        Assert.Contains("DeltaEngine.Integration", references);
        Assert.Contains(references, static name => name.StartsWith("Microsoft.CodeAnalysis", StringComparison.Ordinal));
        Assert.DoesNotContain(references, static name => name.Equals("DeltaEngine", StringComparison.Ordinal));
    }
}

