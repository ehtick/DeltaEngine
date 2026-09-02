# ComputeSmoke (retired)

This project is retained only as a compile-safe migration marker. The retired
standalone compute submission path and its generated fixture are not present
in this checkout.

The replacement path is a Delta.Render RenderGraph sample that receives a
generated `Delta.Shader.Contract.ShaderArtifact` containing SPIR-V, an entry
point and `ShaderAbi`. Do not add another compute submission or manifest path
inside DeltaEngine.
