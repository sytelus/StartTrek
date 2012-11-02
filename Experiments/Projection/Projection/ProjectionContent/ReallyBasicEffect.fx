float4x4 World;
float4x4 View;
float4x4 Projection;

struct VertexShaderInput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION;
	float4 Color    : COLOR0;
};

struct PixelShaderOutput
{
    float4 Color    : COLOR0;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    output.Position = mul(mul(mul(input.Position, World), View), Projection);
	output.Color = input.Color;

    return output;
}


PixelShaderOutput PixelShaderFunction(VertexShaderOutput input)
{
	PixelShaderOutput output;
	output.Color = input.Color;

    return output;
}

technique MainTechnique
{
    pass MainPass
    {
        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
