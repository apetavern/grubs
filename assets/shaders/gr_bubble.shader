
HEADER
{
	Description = "";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	Forward();
	Depth();
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 0
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float4 vColor : COLOR0 < Semantic( Color ); >;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	float3 vPositionOs : TEXCOORD14;
	float3 vNormalOs : TEXCOORD15;
	float4 vTangentUOs_flTangentVSign : TANGENT	< Semantic( TangentU_SignV ); >;
	float4 vColor : COLOR0;
	float4 vTintColor : COLOR1;
	#if ( PROGRAM == VFX_PROGRAM_PS )
		bool vFrontFacing : SV_IsFrontFace;
	#endif
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v.nInstanceTransformID );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	RenderState( CullMode, F_RENDER_BACKFACES ? NONE : DEFAULT );
		
	float4 g_vColorA < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.91, 1.00, 0.35, 1.00 ); >;
	float4 g_vColorB < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 0.00, 0.13, 0.03, 1.00 ); >;
	float g_flFrenelPower < UiGroup( ",0/,0/0" ); Default1( 0.35 ); Range1( 0, 10 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{

		
		float4 l_0 = g_vColorA;
		float4 l_1 = g_vColorB;
		float l_2 = g_flFrenelPower;
		float3 l_3 = pow( 1.0 - dot( normalize( i.vNormalWs ), normalize( CalculatePositionToCameraDirWs( i.vPositionWithOffsetWs.xyz + g_vHighPrecisionLightingOffsetWs.xyz ) ) ), l_2 );
		float4 l_4 = lerp( l_0, l_1, float4( l_3, 0 ) );
		

		return float4( l_4.xyz, 1 );
	}
}
