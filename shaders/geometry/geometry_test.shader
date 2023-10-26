//=========================================================================================================================
// Optional
//=========================================================================================================================
HEADER
{
	Description = "Test Geometry Shader for S&box";
}

//=========================================================================================================================
// Optional
//=========================================================================================================================
FEATURES
{
    #include "common/features.hlsl"
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

struct GeometryInput
{
	
};

//=========================================================================================================================

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( INSTANCED_SHADER_PARAMS( VertexInput i ) )
	{
		PixelInput o = ProcessVertex( i );
		// Add your vertex manipulation functions here
		return FinalizeVertex( o );
	}
}

//=========================================================================================================================
GS 
{
	[maxvertexcount(12)]
	void MainGs(triangle VertexInput input[3], inout TriangleStream<PixelInput> ts) 
	{
		PixelInput o;

		float3 vA = input[1].vPositionOs - input[0].vPositionOs;
		float3 vB = input[2].vPositionOs - input[0].vPositionOs;
		float3 vNormal = cross(vA, vB);

		vNormal = normalize(mul(vNormal, (float3x3)mul(g_matWorldToProjection, vNormal)));
	}
}
//=========================================================================================================================

PS
{
    #include "common/pixel.hlsl"

	float4 MainPs(PixelInput i) : SV_Target0
	{
		return float4(1.0, 0.0, 0.0, 1.0);
	}
}
