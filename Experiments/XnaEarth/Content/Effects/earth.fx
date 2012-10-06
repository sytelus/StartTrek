//-----------------------------------------------------------------------------
// Copyright (c) 2009-2011 dhpoware. All Rights Reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a
// copy of this software and associated documentation files (the "Software"),
// to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense,
// and/or sell copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
// OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS
// IN THE SOFTWARE.
//-----------------------------------------------------------------------------

//-----------------------------------------------------------------------------
// Globals.
//-----------------------------------------------------------------------------

float4x4 world : WORLD;
float4x4 view : VIEW;
float4x4 projection : PROJECTION;

float4 cameraPos;
float4 globalAmbient;
float4 lightDir;
float4 lightColor;
float4 materialAmbient;
float4 materialDiffuse;
float4 materialSpecular;
float materialShininess;
float cloudStrength;

//-----------------------------------------------------------------------------
// Textures.
//-----------------------------------------------------------------------------

texture2D landOceanColorGlossMap;
sampler landOceanColorGlossMapSampler = sampler_state
{
	Texture = <landOceanColorGlossMap>;
    MagFilter = Linear;
    MinFilter = Anisotropic;
    MipFilter = Linear;
    MaxAnisotropy = 16;
};

texture2D cloudColorMap;
sampler cloudColorMapSampler = sampler_state
{
	Texture = <cloudColorMap>;
    MagFilter = Linear;
    MinFilter = Anisotropic;
    MipFilter = Linear;
    MaxAnisotropy = 16;
};

texture2D nightColorMap;
sampler nightColorMapSampler = sampler_state
{
	Texture = <nightColorMap>;
    MagFilter = Linear;
    MinFilter = Anisotropic;
    MipFilter = Linear;
    MaxAnisotropy = 16;
};

texture2D normalMap;
sampler normalMapSampler = sampler_state
{
    Texture = <normalMap>;
    MagFilter = Linear;
    MinFilter = Anisotropic;
    MipFilter = Linear;
    MaxAnisotropy = 16;
};

//-----------------------------------------------------------------------------
// Vertex Shaders.
//-----------------------------------------------------------------------------

void VS_Main(in  float4 inPosition  : POSITION,
             in  float2 inTexCoord  : TEXCOORD,
			 in  float3 inNormal    : NORMAL,
			 in  float3 inTangent   : TANGENT,
			 in  float3 inBitangent : BINORMAL,
			 out float4 outPosition : POSITION,
			 out float2 outTexCoord : TEXCOORD0,
			 out float3 outLightDir : TEXCOORD1,
			 out float3 outViewDir  : TEXCOORD2,
			 out float3 outNormal   : TEXCOORD3)
{
	float4 worldPos = mul(inPosition, world);

    float3 n = mul(inNormal, (float3x3)world);
	float3 t = mul(inTangent, (float3x3)world);
	float3 b = mul(inBitangent, (float3x3)world);
	float3x3 tbnMatrix = float3x3(t.x, b.x, n.x,
	                              t.y, b.y, n.y,
	                              t.z, b.z, n.z);	
	
	outPosition = mul(mul(worldPos, view), projection);
	outTexCoord = inTexCoord;
	outLightDir = mul(-lightDir.xyz, tbnMatrix);
	outViewDir = mul((cameraPos - worldPos).xyz, tbnMatrix);
	outNormal = n;
}

//-----------------------------------------------------------------------------
// Pixel Shaders.
//-----------------------------------------------------------------------------

float4 PS_Mix(in const float4 x, in const float4 y, in const float a)
{
	return x * (1.0f - a) + y * a;
}

void PS_Main(in  float2 inTexCoord : TEXCOORD0,
			 in  float3 inLightDir : TEXCOORD1,
			 in  float3 inViewDir  : TEXCOORD2,
			 in  float3 inNormal   : TEXCOORD3,
			 out float4 outColor   : COLOR,
			 uniform bool bClouds)
{   
    // normalMapSampler is in DXT5nm format.
    // X component in the Alpha and the Y component in the Green channel.
    // We can ignore the Blue channel since we reconstruct the Z component.
    float3 n = tex2D(normalMapSampler, inTexCoord).agb * 2.0f - 1.0f;
    n.z = sqrt(1.0f - n.x * n.x - n.y * n.y);
    n = normalize(n);
    
    //float3 n = normalize(tex2D(normalMapSampler, inTexCoord).rgb * 2.0f - 1.0f);
    float3 l = normalize(inLightDir);
    float3 v = normalize(inViewDir);
    float3 h = normalize(l + v);

	float nDotL = saturate(dot(n, l));
	float nDotH = saturate(dot(n, h));
	float power = (nDotL <= 0.0f) ? 0.0f : pow(nDotH, materialShininess);
			
	float4 ambient = (materialAmbient * (globalAmbient + lightColor));
	float4 diffuse = (materialDiffuse * lightColor * nDotL);
	float4 specular = (materialSpecular * lightColor * power);
	
	float selfShadow = saturate(4.0f * l.z);
	float4 landOceanSample = tex2D(landOceanColorGlossMapSampler, inTexCoord);
	float4 day = float4(landOceanSample.rgb, 1.0f) * (ambient + selfShadow * (diffuse + specular * landOceanSample.a));
	float4 night = tex2D(nightColorMapSampler, inTexCoord) * (ambient + selfShadow * diffuse);
		
	if (bClouds)
	{
		float cloud = tex2D(cloudColorMapSampler, inTexCoord).r;
		float cloudDiffuse = saturate(dot(normalize(inNormal), normalize(-lightDir.xyz)));
		
		day = day * (1.0f - cloud) + (cloud * cloudDiffuse * cloudStrength);
		night = night * (1.0f - cloud) * cloudStrength;
	}
		
	outColor = day;

	if (nDotL < 0.1f)
		outColor = PS_Mix(night, day, (nDotL + 0.1f) * 5.0f);
}

//-----------------------------------------------------------------------------
// Techniques.
//-----------------------------------------------------------------------------

technique EarthWithClouds
{
	pass
	{
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main(true);
	}
}

technique EarthWithoutClouds
{
	pass
	{
		VertexShader = compile vs_2_0 VS_Main();
		PixelShader = compile ps_2_0 PS_Main(false);
	}
}