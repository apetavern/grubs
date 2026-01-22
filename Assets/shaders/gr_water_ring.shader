
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
	#define S_ALPHA_TEST 1
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
		
	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Texture1, Srgb, 8, "None", "_color", ",0/,0/0", DefaultFile( "particles/watersplash/textures/water_ring.png" ) );
	CreateInputTexture2D( Texture2, Srgb, 8, "None", "_color", ",0/,0/0", DefaultFile( "particles/watersplash/textures/water_ring.png" ) );
	Texture2D g_tTexture1 < Channel( RGBA, Box( Texture1 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	Texture2D g_tTexture2 < Channel( RGBA, Box( Texture2 ), Srgb ); OutputFormat( DXT5 ); SrgbRead( True ); >;
	float4 g_vColor < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flEmissionStrength < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 10 ); >;
	float g_flSmoothStepMin < UiGroup( ",0/,0/0" ); Default1( 0.25 ); Range1( 0, 1 ); >;
	float g_flSmoothStepMax < UiGroup( ",0/,0/1" ); Default1( 0.35 ); Range1( 0, 1 ); >;
	float2 g_vUVTilingOne < UiGroup( ",0/,0/0" ); Default2( 10,1 ); Range2( 0,0, 20,20 ); >;
	float g_flSpeedOne < UiGroup( ",0/,0/0" ); Default1( 0.1 ); Range1( 0, 1 ); >;
	float2 g_vUVTilingTwo < UiGroup( ",0/,0/0" ); Default2( 16,1 ); Range2( 0,0, 20,20 ); >;
	float g_flSpeedTwo < UiGroup( ",0/,0/0" ); Default1( 0.2 ); Range1( 0, 1 ); >;
	
	float4 MainPs( PixelInput i ) : SV_Target0
	{
		
		Material m = Material::Init( i );
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
		float l_1 = g_flEmissionStrength;
		float4 l_2 = l_0 * float4( l_1, l_1, l_1, l_1 );
		float l_3 = g_flSmoothStepMin;
		float l_4 = g_flSmoothStepMax;
		float2 l_5 = i.vTextureCoords.xy * float2( 1, 1 );
		float2 l_6 = g_vUVTilingOne;
		float l_7 = g_flSpeedOne;
		float l_8 = l_7 * g_flTime;
		float l_9 = 1 - l_8;
		float2 l_10 = TileAndOffsetUv( l_5, l_6, float2( l_9, l_9 ) );
		float4 l_11 = Tex2DS( g_tTexture1, g_sSampler0, l_10 );
		float2 l_12 = g_vUVTilingTwo;
		float l_13 = g_flSpeedTwo;
		float l_14 = l_13 * g_flTime;
		float l_15 = 1 - l_14;
		float2 l_16 = TileAndOffsetUv( l_5, l_12, float2( l_15, l_15 ) );
		float4 l_17 = Tex2DS( g_tTexture2, g_sSampler0, l_16 );
		float4 l_18 = lerp( l_11, l_17, 0.5 );
		float4 l_19 = smoothstep( l_3, l_4, l_18 );
		
		m.Albedo = l_0.xyz;
		m.Emission = l_2.xyz;
		m.Opacity = l_19.x;
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
				
		return ShadingModelStandard::Shade( m );
	}
}
