
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
	Depth( S_MODE_DEPTH );
	ToolsShadingComplexity( "tools_shading_complexity.shader" );
}

COMMON
{
	#ifndef S_ALPHA_TEST
	#define S_ALPHA_TEST 0
	#endif
	#ifndef S_TRANSLUCENT
	#define S_TRANSLUCENT 1
	#endif
	
	#include "common/shared.hlsl"
	#include "procedural.hlsl"

	#define S_UV2 1
	#define CUSTOM_MATERIAL_INPUTS
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
};

VS
{
	#include "common/vertex.hlsl"
	
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( PosOffsetTexture, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tPosOffsetTexture < Channel( RGBA, Box( PosOffsetTexture ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float2 g_vUVTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( -10,-10, 10,10 ); >;
	float g_flScrollspeed < UiGroup( ",0/,0/0" ); Default1( 2 ); Range1( -50, 50 ); >;
	float g_flWPOStrength < UiGroup( ",0/,0/0" ); Default1( 6 ); Range1( -50, 50 ); >;
	
	PixelInput MainVs( VertexInput v )
	{
		
		PixelInput i = ProcessVertex( v );
		i.vPositionOs = v.vPositionOs.xyz;
		i.vColor = v.vColor;
		
		ExtraShaderData_t extraShaderData = GetExtraPerInstanceShaderData( v );
		i.vTintColor = extraShaderData.vTint;
		
		VS_DecodeObjectSpaceNormalAndTangent( v, i.vNormalOs, i.vTangentUOs_flTangentVSign );
		
		float2 l_0 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_1 = g_vUVTiling;
		float l_2 = g_flScrollspeed;
		float l_3 = g_flTime * l_2;
		float l_4 = 1 - l_3;
		float l_5 = l_4.x;
		float4 l_6 = float4( l_5, 0, 0, 0 );
		float2 l_7 = TileAndOffsetUv( l_0, l_1, l_6.xy );
		float4 l_8 = g_tPosOffsetTexture.SampleLevel( g_sSampler0, l_7, 0 );
		float l_9 = g_flWPOStrength;
		float l_10 = l_8.r * l_9;
		float3 l_11 = float3( l_10, l_10, l_10 ) * i.vNormalOs;
		i.vPositionWs.xyz += l_11;
		i.vPositionPs.xyzw = Position3WsToPs( i.vPositionWs.xyz );
		return FinalizeVertex( i );
		
	}
}

PS
{
	#include "common/pixel.hlsl"
	
	DynamicCombo( D_RENDER_BACKFACES, 0..1, Sys( ALL ) );
	RenderState( CullMode, D_RENDER_BACKFACES ? NONE : BACK );
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( PosOffsetTexture, Linear, 8, "None", "_color", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	Texture2D g_tPosOffsetTexture < Channel( RGBA, Box( PosOffsetTexture ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float4 g_vColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 0.13, 0.00, 1.00 ); >;
	float g_flGlowStrength < UiGroup( ",0/,0/0" ); Default1( 50 ); Range1( 0, 100 ); >;
	float2 g_vUVTiling < UiGroup( ",0/,0/0" ); Default2( 1,1 ); Range2( -10,-10, 10,10 ); >;
	float g_flScrollspeed < UiGroup( ",0/,0/0" ); Default1( 2 ); Range1( -50, 50 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init();
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = float3( 0, 0, 1 );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;
		
		float4 l_0 = g_vColor;
		float4 l_1 = i.vTintColor;
		float4 l_2 = l_0 * l_1;
		float l_3 = g_flGlowStrength;
		float4 l_4 = l_2 * float4( l_3, l_3, l_3, l_3 );
		float2 l_5 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_6 = g_vUVTiling;
		float l_7 = g_flScrollspeed;
		float l_8 = g_flTime * l_7;
		float l_9 = 1 - l_8;
		float l_10 = l_9.x;
		float4 l_11 = float4( l_10, 0, 0, 0 );
		float2 l_12 = TileAndOffsetUv( l_5, l_6, l_11.xy );
		float4 l_13 = Tex2DS( g_tPosOffsetTexture, g_sSampler0, l_12 );
		float3 l_14 = i.vColor.rgb;
		float3 l_15 = float3( l_13.r, l_13.r, l_13.r ) * l_14;
		
		m.Albedo = l_4.xyz;
		m.Opacity = l_15.x;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		
		
		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		// Result node takes normal as tangent space, convert it to world space now
		m.Normal = TransformNormal( m.Normal, i.vNormalWs, i.vTangentUWs, i.vTangentVWs );
		
		// for some toolvis shit
		m.WorldTangentU = i.vTangentUWs;
		m.WorldTangentV = i.vTangentVWs;
		m.TextureCoords = i.vTextureCoords.xy;
				
		return ShadingModelStandard::Shade( i, m );
	}
}
