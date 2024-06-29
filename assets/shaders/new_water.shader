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
	ToolsVis( S_MODE_TOOLS_VIS );
	ToolsWireframe( "vr_tools_wireframe.shader" );
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

}

struct VertexInput
{
	#include "common/vertexinput.hlsl"
	float3 normal : normal;
};

struct PixelInput
{
	#include "common/pixelinput.hlsl"
	
	float3 positionInWorldSpace : TEXCOORD15;
	float3 CameraToPositionRay	: TEXCOORD16;
	float3 normal : normal;  // Use NORMAL instead of TEXCOORD17 for automatic interpolation
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


	PixelInput MainVs(VertexInput i)
	{
		PixelInput o = ProcessVertex(i);

		// Do wave height
		o.vPositionWs.z += CalculateWaveHeight(o);
		o.vPositionPs.xyzw = Position3WsToPs(o.vPositionWs.xyz);

		// Copy the vertex position so the ps can access it
		o.positionInWorldSpace = o.vPositionWs;

		o.CameraToPositionRay.xyz = CalculateCameraToPositionRayWs(o.vPositionWs.xyz);

		// Pass the vertex normal to the pixel shader
		o.normal = i.normal;

		return FinalizeVertex(o);
	}


}

PS
{
	#include "common/pixel.hlsl"

	#include "vr_lighting.fxc"

	#define BLEND_MODE_ALREADY_SET
	// Disable blend state settings for transparency
    RenderState( BlendEnable, false );

	#define DEPTH_STATE_ALREADY_SET
	RenderState( DepthWriteEnable, true ); // Enable depth writing for proper depth testing

	float2 g_flLightbias < Default2( 0.5, 0.5 ); Range2( 0, 0, 1, 1 ); UiGroup( "Water,0/,0/0" ); >;

	float g_flDeepWaterColorHeight < UiGroup( "Water,0/,0/0" ); Range( 0, 50 ); Default( 25 ); >;

	float4 g_flDeepWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.17, 0.46, 0.81, 1 ); >;

	float4 g_flShallowWaterColor < UiType( Color ); UiGroup( "Water,0/,0/0" ); Default4( 0.50, 0.65, 0.80, 1.0 ); >;

	float g_flDepthMaxDistance < UiGroup( "Water,0/,0/0" ); Range( 0, 200 ); Default( 65 ); >;

	float g_flPolarizeStepCount < UiGroup( "Water,0/,0/0" ); Range( 0, 100 ); Default( 10 ); >;

	float g_flFoamDepth < UiGroup( "Foam,0/,0/0" ); Range( 0, 100 ); Default( 25 ); >;
	
	float g_flFoamFade < UiGroup( "Foam,0/,0/0" ); Range( 0, 0.9 ); Default( 0.2 ); >;

	float3 g_flSpecularColor < Default3(1.0, 1.0, 1.0); UiType(Color); UiGroup("Specular,0/,0/0"); >;
	float g_flSpecularIntensity < Default(0.5); Range(0, 1000); UiGroup("Specular,0/,0/0"); >;
	float g_flSpecularPower < Default(32); Range(1, 128); UiGroup("Specular,0/,0/0"); >;

    // New foam color parameter
    float4 g_flFoamColor < UiType( Color ); UiGroup( "Foam,0/,0/0" ); Default4( 1.0, 1.0, 1.0, 1.0 ); >;

	float3 CalculateLighting( float3 normal )
	{
		float3 lightDirection = normalize( BinnedLightBuffer[0].GetPosition() );
		float3 lightColour = BinnedLightBuffer[0].GetColor();

		float brightness = max( dot( lightDirection, normal ), 0.0f );
		return ( lightColour * g_flLightbias.x ) + ( brightness * lightColour * g_flLightbias.y );
	}

	float3 Polarize ( float3 color, int step ) 
	{
		return color;//round( color * step ) / step;
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

		float4 deepWaterColor = float4( SrgbGammaToLinear( g_flDeepWaterColor.rgb ), 1.0 ); // Set alpha to 1
		float4 shallowWaterColor = float4( SrgbGammaToLinear( g_flShallowWaterColor.rgb ), 1.0 ); // Set alpha to 1
		float4 lerpedColour = lerp( shallowWaterColor, deepWaterColor, waterDepth );

		return float4( lerpedColour.rgb, 1.0 ); // Ensure alpha is 1
	}

	float4 CalculateIntersectionFoam( float4 vPositionSs, float3 positionInWorldSpace, float3 cameraRay ) 
	{
		float depth = CalculateWorldSpacePosition( cameraRay, vPositionSs.xy ).z;
		float differenceInHeight = positionInWorldSpace.z - depth;

		// Create the depth fade mask and do one minus so the intersection is white
		float depthFadeMask01 = 1 - saturate( differenceInHeight / g_flFoamDepth );
		
		// Add a smoothsection to the depth fade mask so we can get a intersection mask that is smooth
		float intersectionMask01 = smoothstep( 1 - g_flFoamFade, 1, depthFadeMask01 + 0.1 );
		
		// Use foam color
		return float4( g_flFoamColor.rgb * intersectionMask01, 1.0 );
	}

	float4 CalculateRandomSurfaceFoam( float3 positionInWorldSpace ) {
		
		float2 offsetCoordinatesBy = float2( g_flTime * 2, g_flTime * 2 );
		float2 tileOffset = TileAndOffsetUv( positionInWorldSpace.xy, float2( 0.5, 0.5 ), offsetCoordinatesBy );
		float noise = VoronoiNoise( tileOffset * 0.003, g_flTime * 1, 1 ) * 0.025;
		// float noiseStepped = step( noise, 0.01 );
		return float4( g_flFoamColor.rgb * noise, 1.0 ); // Use foam color
	}

	// https://ameye.dev/notes/stylized-water-shader/ and https://roystan.net/articles/toon-water/
	float4 MainPs(PixelInput i) : SV_Target0
	{
		// Lerp the colour from deep wave color to shallow wave color depending on how deep the water is		
		float4 waveColor = CalculateWaveColor(i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay);

		// Do some foam where the water intersects with the terrain and add it to the current wave colour
		float4 intersectionFoam = CalculateIntersectionFoam(i.vPositionSs, i.positionInWorldSpace, i.CameraToPositionRay);
		waveColor = saturate(waveColor + intersectionFoam);

		// Add random surface foam
		float4 randomFoam = CalculateRandomSurfaceFoam(i.positionInWorldSpace);
		waveColor = saturate(waveColor + randomFoam);

		// Use the interpolated normal for smooth shading
		float3 calculatedNormal = normalize(i.normal);

		// Add specular reflection
		float3 viewDir = normalize(-i.CameraToPositionRay);
		float3 lightDir = normalize(BinnedLightBuffer[0].GetPosition());
		float3 reflectDir = reflect(lightDir, calculatedNormal);
		float spec = pow(max(dot(viewDir, reflectDir), 0.0), g_flSpecularPower * 2);
		float3 specular = (g_flSpecularColor * spec * g_flSpecularIntensity * intersectionFoam * 10 * (randomFoam*waveColor) * intersectionFoam * 10) + intersectionFoam*2 ;

		// Calculate lighting and add specular reflection
		float4 waveColorLit = float4(waveColor.rgb * CalculateLighting(calculatedNormal) + specular, 1.0); // Ensure alpha is 1

		return waveColorLit;
	}


}
