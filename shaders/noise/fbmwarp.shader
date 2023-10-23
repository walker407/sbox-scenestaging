/*
    Based on https://thebookofshaders.com/13/
*/

//=========================================================================================================================
HEADER
{
	Description = "Domain Warp Fractal Noise Shader for S&box";
}

//=========================================================================================================================

FEATURES
{
    #include "common/features.hlsl"
}

MODES
{
    VrForward();													// Indicates this shader will be used for main rendering
    ToolsVis( S_MODE_TOOLS_VIS );									// Ability to see in the editor
    ToolsWireframe("vr_tools_wireframe.shader");					// Allows for mat_wireframe to work
    ToolsShadingComplexity("tools_shading_complexity.shader");		// Shows how expensive drawing is in debug view
}

//=========================================================================================================================
COMMON
{
	#include "common/shared.hlsl"

    
}

//=========================================================================================================================

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

//=========================================================================================================================

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );
	
		return FinalizeVertex( i );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

    int g_Octaves <  UiType(Slider); UiStep(1); Default(10); Range(1, 10);>;
    int g_Lacunarity <  UiType(Slider); UiStep(1); Default(2); Range(1, 16);>;
    float g_Scale <  UiType(Slider); UiStep(0.1); Default(8); Range(0.1, 128);>;
    float g_Gain <  UiType(Slider); UiStep(0.1); Default(0.5); Range(0, 1);>;

    float3 g_vColor1 < UiType( Color ); Default3(0.101961,0.619608,0.666667); >;
    float3 g_vColor2 < UiType( Color ); Default3(0.666667,0.666667,0.498039); >;

    float3 g_vColor3 < UiType( Color ); Default3(0,0,0.164706); >;
    float3 g_vColor4 < UiType( Color ); Default3(0.666667, 1.0, 1.0); >;
    
    float random(in float2 st) 
    {
        return frac(sin(dot(st.xy, float2(12.9898, 78.233))) * 43758.5453123);
    }

    float noise (in float2 st) 
    {
        float2 i = floor(st);
        float2 f = frac(st);

        float a = random(i);
        float b = random(i + float2(1.0, 0.0));
        float c = random(i + float2(0.0, 1.0));
        float d = random(i + float2(1.0, 1.0));

        float2 u = f * f * (3.0 - 2.0 * f);

        return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
    }

    float fbm(in float2 st) 
    {
        float value = 0.0;
        float amplitude = 0.5f;
        float2 shift = float2(100.0, 100.0);

        float2x2 rotate = float2x2(cos(0.5), sin(0.5), -sin(0.5), cos(0.5));
        
        [unroll(10)]
        for(int i=0; i < g_Octaves; i++) 
        {
            value += amplitude * noise(st);
            st = mul(rotate, st * g_Lacunarity + shift);
            amplitude *= g_Gain;
        }

        return value;
    }

	float4 MainPs( PixelInput i) : SV_Target0
	{
        float2 st = i.vTextureCoords.xy * g_Scale;

        float3 color = float3(0,0,0);

        float2 q = float2(0,0);
        q.x = fbm(st + 0.00 * g_flTime);
        q.y = fbm(st + float2(1.0, 1.0));

        float2 r = float2(0,0);
        r.x = fbm(st + 1.0 * q + float2(1.7, 9.2) + 0.15 * g_flTime);
        r.y = fbm(st + 1.0 * q + float2(8.3, 2.8) + 0.126 * g_flTime);

        float f = fbm(st + r);

        color = lerp(g_vColor1,
                     g_vColor2,
                     clamp((f*f) * 4.0, 0.0, 1.0));

        color = lerp(color,g_vColor3 , clamp(length(q), 0.0, 1.0));

        color = lerp(color, g_vColor4, clamp(length(r.x), 0.0, 1.0));

		return float4((f*f*f+ 0.6*f*f+.5*f) * color, 1);
	}
}
