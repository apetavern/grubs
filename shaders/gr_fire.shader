
HEADER
{
	Description = "";
}

FEATURES
{
	#include "vr_common_features.fxc"
	Feature( F_ADDITIVE_BLEND, 0..1, "Blending" );
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

	#define S_UV2 1
}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
};

VS
{
	#include "common/vertex.hlsl"

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );
		return FinalizeVertex( o );
	}
}

PS
{
	#include "sbox_pixel.fxc"
	#include "common/pixel.material.structs.hlsl"
	#include "common/pixel.lighting.hlsl"
	#include "common/pixel.shading.hlsl"
	#include "common/pixel.material.helpers.hlsl"
	#include "common/pixel.color.blending.hlsl"
	#include "common/proceedural.hlsl"

	SamplerState g_sSampler0 < Filter( ANISO ); AddressU( CLAMP ); AddressV( CLAMP ); >;
	SamplerState g_sSampler1 < Filter( ANISO ); AddressU( WRAP ); AddressV( WRAP ); >;
	CreateInputTexture2D( Gradient, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( NoiseOne, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( NoiseTwo, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( DistortionMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateInputTexture2D( FlameMask, Linear, 8, "None", "_mask", ",0/,0/0", Default4( 1.00, 1.00, 1.00, 1.00 ) );
	CreateTexture2DWithoutSampler( g_tGradient ) < Channel( RGBA, Box( Gradient ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tNoiseOne ) < Channel( RGBA, Box( NoiseOne ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tNoiseTwo ) < Channel( RGBA, Box( NoiseTwo ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tDistortionMask ) < Channel( RGBA, Box( DistortionMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	CreateTexture2DWithoutSampler( g_tFlameMask ) < Channel( RGBA, Box( FlameMask ), Linear ); OutputFormat( DXT5 ); SrgbRead( False ); >;
	float g_flNoiseOnePanSpeed < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 5 ); >;
	float g_flNoiseTwoPanSpeed < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 5 ); >;
	float g_flUVDistortionIntensity < UiGroup( ",0/,0/0" ); Default1( 0.5 ); Range1( 0, 1 ); >;
	float g_flFlameMaskDistortion < UiGroup( ",0/,0/0" ); Default1( 0.05 ); Range1( 0, 1 ); >;
	float4 g_vColour < UiType( Color ); UiGroup( ",0/,0/0" ); Default4( 1.00, 1.00, 1.00, 1.00 ); >;
	float g_flEmissionStrength < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 100 ); >;
	float g_flSmoothStepIn < UiGroup( ",0/,0/0" ); Default1( 0 ); Range1( 0, 1 ); >;
	float g_flSmoothStepOut < UiGroup( ",0/,0/0" ); Default1( 1 ); Range1( 0, 1 ); >;

	float4 MainPs( PixelInput i ) : SV_Target0
	{
		Material m;
		m.Albedo = float3( 1, 1, 1 );
		m.Normal = TransformNormal( i, float3( 0, 0, 1 ) );
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;
		m.TintMask = 1;
		m.Opacity = 1;
		m.Emission = float3( 0, 0, 0 );
		m.Transmission = 0;

		float2 local0 = i.vTextureCoords.xy * float2( 1, 1 );
		float local1 = local0.x;
		float local2 = g_flNoiseOnePanSpeed;
		float local3 = local2 * g_flTime;
		float local4 = local0.y;
		float local5 = local3 + local4;
		float4 local6 = float4( local1, local5, 0, 0 );
		float4 local7 = Tex2DS( g_tNoiseOne, g_sSampler1, local6.xy );
		float2 local8 = i.vTextureCoords.xy * float2( 1, 1 );
		float local9 = local8.x;
		float local10 = g_flNoiseTwoPanSpeed;
		float local11 = local10 * g_flTime;
		float local12 = local8.y;
		float local13 = local11 + local12;
		float4 local14 = float4( local9, local13, 0, 0 );
		float4 local15 = Tex2DS( g_tNoiseTwo, g_sSampler1, local14.xy );
		float4 local16 = local7 + local15;
		float4 local17 = Tex2DS( g_tDistortionMask, g_sSampler1, i.vTextureCoords.xy );
		float4 local18 = local16 * local17;
		float local19 = g_flUVDistortionIntensity;
		float4 local20 = local18 * float4( local19, local19, local19, local19 );
		float2 local21 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local22 = local20 + float4( local21.xy, 0, 0 );
		float4 local23 = Tex2DS( g_tGradient, g_sSampler0, local22.xy );
		float local24 = g_flFlameMaskDistortion;
		float4 local25 = local16 * float4( local24, local24, local24, local24 );
		float2 local26 = i.vTextureCoords.xy * float2( 1, 1 );
		float4 local27 = local25 + float4( local26.xy, 0, 0 );
		float4 local28 = Tex2DS( g_tFlameMask, g_sSampler1, local27.xy );
		float4 local29 = local23 * local28;
		float4 local30 = g_vColour;
		float4 local31 = local29 * local30;
		float local32 = g_flEmissionStrength;
		float4 local33 = local31 * float4( local32, local32, local32, local32 );
		float local34 = g_flSmoothStepIn;
		float local35 = g_flSmoothStepOut;
		float4 local36 = smoothstep( local34, local35, local29 );
		float4 local37 = saturate( local36 );

		m.Albedo = local31.xyz;
		m.Emission = local33.xyz;
		m.Opacity = local37.x;
		m.Roughness = 1;
		m.Metalness = 0;
		m.AmbientOcclusion = 1;

		m.AmbientOcclusion = saturate( m.AmbientOcclusion );
		m.Roughness = saturate( m.Roughness );
		m.Metalness = saturate( m.Metalness );
		m.Opacity = saturate( m.Opacity );
		
		ShadingModelValveStandard sm;
		return FinalizePixelMaterial( i, m, sm );
	}
}
