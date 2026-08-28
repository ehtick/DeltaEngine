using System.Collections.Generic;
using Delta.Maths;
using System.Text.Json.Serialization;

namespace Delta.Engine.Assets;

[System.Diagnostics.CodeAnalysis.SuppressMessage(
    "Design",
    "CA1051:Do not declare visible instance fields",
    Justification = "Material fields are the serialized editor/runtime asset ABI.")]
public class MaterialData : IAsset
{
    public readonly GuidAsset<ShaderData> shader;

    [JsonIgnore]
    public Dictionary<string, float> _floatValues = [];
    [JsonIgnore]
    public Dictionary<string, float2> _vector2Values = [];
    [JsonIgnore]
    public Dictionary<string, float3> _vector3Values = [];
    [JsonIgnore]
    public Dictionary<string, float4> _vector4Values = [];

    public MaterialData(GuidAsset<ShaderData> shader)
    {
        this.shader = shader;
    }
}
