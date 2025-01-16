// Original credit: Waisie Milliams Hah, TerryScape Water Shader
HEADER
{
	Description = "Water shader for Pirates";
}

FEATURES
{
	#include "common/features.hlsl"
}

MODES
{
	VrForward();
	Depth(); 
	ToolsWireframe( "vr_tools_wireframe.shader" );
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

}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	
	float3 positionInWorldSpace : TEXCOORD15;
	float3 CameraToPositionRay	: TEXCOORD16;
};

VS
{
	#include "common/vertex.hlsl"
		
	float g_flWaveScrollSpeed < UiGroup( "Water,0/,0/0" ); Default1( 0.21 ); Range1( 0, 1 ); >;

	float g_flWaveScale < UiGroup( "Water,0/,0/0" ); Default1( 14 ); Range1( 0, 50 ); >;

	float CalculateWaveHeight( PixelInput o )
	{
		float2 offsetCoordinatesBy = float2( g_flTime * g_flWaveScrollSpeed, g_flTime * g_flWaveScrollSpeed );
		float2 tileOffset = TileAndOffsetUv( o.vPositionWs.xy, float2( 1, 1 ), offsetCoordinatesBy );

		float noiseValue = Simplex2D( tileOffset );

		return saturate( noiseValue ) * g_flWaveScale;
	}

	PixelInput MainVs( VertexInput i )
	{
		PixelInput o = ProcessVertex( i );

		// Do wave height
		o.vPositionWs.z += CalculateWaveHeight( o );	
		o.vPositionPs.xyzw = Position3WsToPs( o.vPositionWs.xyz );
		
		// Copy the vertex position so the ps can access it
		o.positionInWorldSpace = o.vPositionWs;

		o.CameraToPositionRay.xyz = CalculateCameraToPositionRayWs( o.vPositionWs.xyz );

		return FinalizeVertex( o );
	}
}

PS
{
	#include "common/pixel.hlsl"

	#include "vr_lighting.fxc"

	#define BLEND_MODE_ALREADY_SET
    RenderState( BlendEnable, true );
    RenderState( BlendOp, ADD );
    RenderState( SrcBlend, SRC_ALPHA );
    RenderState( DstBlend, INV_SRC_ALPHA );

	#define DEPTH_STATE_ALREADY_SET
	RenderState( DepthWriteEnable, false );

	float2 g_flLightbias < Default2( 0.5, 0.5 ); Range2( 0, 0, 1, 1 ); UiGroup( "Water,0/,0/0" ); >;

	float g_flDeepWaterColorHeight < UiGroup( "Water,0/,0/0" ); Range( 0, 50 ); Default( 25 ); >;

	float4 g_flDeepWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.17, 0.46, 0.81, 1 ); >;

	float4 g_flShallowWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.50, 0.65, 0.80, 0.40 ); >;

	float g_flDepthMaxDistance < UiGroup( "Water,0/,0/0" ); Range( 0, 200 ); Default( 65 ); >;

	float g_flPolarizeStepCount < UiGroup( "Water,0/,0/0" ); Range( 0, 100 ); Default( 10 ); >;

	float g_flFoamDepth < UiGroup( "Foam,0/,0/0" ); Range( 0, 100 ); Default( 25 ); >;
	
	float g_flFoamFade < UiGroup( "Foam,0/,0/0" ); Range( 0, 0.9 ); Default( 0.2 ); >;

	float3 CalculateLighting( float3 normal )
	{
		float3 lightDirection = normalize( BinnedLightBuffer[0].GetPosition() );
		float3 lightColour = BinnedLightBuffer[0].GetColor();

		float brightness = max( dot( lightDirection, normal ), 0.0f );
		return ( lightColour * g_flLightbias.x ) + ( brightness * lightColour * g_flLightbias.y );
	}

	float3 Polarize ( float3 color, int step ) 
	{
		return round( color * step ) / step;
	}

	float3 CalculateWorldSpacePosition( float3 vCameraToPositionRayWs, float2 vPositionSs )
	{
		float depth = Depth::GetNormalized( vPositionSs.xy );
		return RecoverWorldPosFromProjectedDepthAndRay( depth, vCameraToPositionRayWs );
	}

	float4 CalculateWaveColor( float4 vPositionSs, float3 positionInWorldSpace, float3 cameraRay ) 
	{
		float depth = CalculateWorldSpacePosition( cameraRay, vPositionSs.xy ).z;
		
		float differenceInHeight = positionInWorldSpace.z - depth;
		float waterDepth = saturate( differenceInHeight / g_flDepthMaxDistance );

		float4 deepWaterColor = float4( SrgbGammaToLinear( g_flDeepWaterColor.rgb ), g_flDeepWaterColor.a );
		float4 shallowWaterColor = float4( SrgbGammaToLinear( g_flShallowWaterColor.rgb ), g_flShallowWaterColor.a );
		float4 lerpedColour = lerp( shallowWaterColor, deepWaterColor, waterDepth );

		return float4( lerpedColour );
	}

	float4 CalculateIntersectionFoam( float4 vPositionSs, float3 positionInWorldSpace, float3 cameraRay ) 
	{
		float depth = CalculateWorldSpacePosition( cameraRay, vPositionSs.xy ).z;
		float differenceInHeight = positionInWorldSpace.z - depth;

		// Create the depth fade mask and do one minus so the intersection is white
		float depthFadeMask01 = 1 - saturate( differenceInHeight / g_flFoamDepth );
		
		// Add a smoothsection to the depth fade mask so we can get a intersection mask that is smooth
		float intersectionMask01 = smoothstep( 1 - g_flFoamFade, 1, depthFadeMask01 + 0.1 );
		
		// TODO Do we want some better form?
		return intersectionMask01;
	}

	float4 CalculateRandomSurfaceFoam( float3 positionInWorldSpace ) {
		
		float2 offsetCoordinatesBy = float2( g_flTime * 2, g_flTime * 2 );
		float2 tileOffset = TileAndOffsetUv( positionInWorldSpace.xy, float2( 0.5, 0.5 ), offsetCoordinatesBy );
		float noise = VoronoiNoise( tileOffset * 0.01, g_flTime * 1, 1 ) * 0.025;
		// float noiseStepped = step( noise, 0.01 );
		return float4( noise, noise, noise, 0.5 );
	}

	// https://ameye.dev/notes/stylized-water-shader/ and https://roystan.net/articles/toon-water/
	float4 MainPs( PixelInput i ) : SV_Target0
	{		
		// Lerp the colour from deep wave color to shallow wave color depending on how deep the water is		
		float4 waveColor = CalculateWaveColor( i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay );

		// Do some foam where the water intersects with the terrain and add it to the current wave colour
		float4 intersectionFoam = CalculateIntersectionFoam( i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay );
		waveColor = saturate( waveColor + intersectionFoam );

		// Add random surface foam
		float4 randomFoam = CalculateRandomSurfaceFoam( i.positionInWorldSpace );
		waveColor = saturate( waveColor + randomFoam );

		// Calculate the wave normal and apply a flat shading lighting effect to it
		float3 calculatedNormal = normalize( cross( ddy( i.positionInWorldSpace ), ddx( i.positionInWorldSpace ) ) );
		// float3 calculatedNormal = normalize( sin( ddy( i.positionInWorldSpace ) ) );
		float4 waveColorLit = float4( waveColor.rgb * CalculateLighting( calculatedNormal ), waveColor.a );

		return waveColorLit; 
	}
}