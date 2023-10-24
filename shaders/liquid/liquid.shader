//=========================================================================================================================

HEADER
{
	Description = "Liquid Shader for S&box";
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

	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 1
	#endif

	#define CULL_MODE_ALREADY_SET
    #define DEPTH_STATE_ALREADY_SET
	
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

	float3 vFillPosition : POSITION < Semantic( PosXyz ); >;
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	float g_flWobbleX <Attribute("WobbleX"); Range(-8, 4); Default(0);>;
	float g_flWobbleY <Attribute("WobbleY"); Range(-8, 4); Default(0);>;

	 float3 Unity_RotateAboutAxis_Degrees(float3 In, float3 Axis, float Rotation)
	{
		Rotation = radians(Rotation);
		float s = sin(Rotation);
		float c = cos(Rotation);
		float one_minus_c = 1.0 - c;

		Axis = normalize(Axis);
		float3x3 rot_mat = 
		{   one_minus_c * Axis.x * Axis.x + c, one_minus_c * Axis.x * Axis.y - Axis.z * s, one_minus_c * Axis.z * Axis.x + Axis.y * s,
			one_minus_c * Axis.x * Axis.y + Axis.z * s, one_minus_c * Axis.y * Axis.y + c, one_minus_c * Axis.y * Axis.z - Axis.x * s,
			one_minus_c * Axis.z * Axis.x - Axis.y * s, one_minus_c * Axis.y * Axis.z + Axis.x * s, one_minus_c * Axis.z * Axis.z + c
		};
		float3 Out = mul(rot_mat,  In);
		return Out;
	}

	PixelInput MainVs( VertexInput v )
	{
		PixelInput i = ProcessVertex( v );

		float3 vPositionWs = mul(CalculateInstancingObjectToWorldMatrix( INSTANCING_PARAMS( v ) ), v.vPositionOs.xyz);

		float3 worldPosX= Unity_RotateAboutAxis_Degrees(vPositionWs, float3(1,0,0), 90);
		float3 worldPosY= Unity_RotateAboutAxis_Degrees(vPositionWs, float3(0,1,0), 90);

		i.vFillPosition = normalize(vPositionWs + (worldPosX * g_flWobbleX) + (worldPosY * g_flWobbleY));

		return FinalizeVertex( i );
	}
}

//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

	float g_flFillLevel <UiType( Slider); Range(-1.0, 1.0); Default(0.75);>;
	float3 g_vFillColorUpper < UiType( Color ); Default3(0, 0.5, 0.5); >;
	float3 g_vFillColorLower < UiType( Color ); Default3(0, 0, 1); >;
	float3 g_vFFoamColor < UiType( Color ); Default3(0, 0.6, 0.7); >;

	float4 MainPs( PixelInput i, bool isFrontFace : SV_IsFrontFace ) : SV_Target0
	{
		float3 edge = dot(i.vNormalWs, float3(0,0,1));

		float fill = step(i.vFillPosition.z, 0);
		
		float3 color = lerp(g_vFillColorUpper,  g_vFillColorLower, edge.z );
		float3 colors = lerp(g_vFFoamColor, color, isFrontFace).xyz;
		
		return float4(colors, fill);
	}
}
