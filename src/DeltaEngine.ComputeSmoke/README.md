# ComputeSmoke (retired)

This project is retained only as a compile-safe migration marker. The former
smoke used the removed `Delta.Shader.Abstractions` and `IComputeDevice` APIs,
and its generated fixture is not present in this checkout.

The replacement path is a Delta.Render RenderGraph sample that receives a
generated `Delta.Shader.Contract.ShaderArtifact` containing SPIR-V, an entry
point and `ShaderAbi`. Do not restore the retired device or manifest adapter
inside DeltaEngine.
