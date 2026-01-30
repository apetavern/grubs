using Sandbox.Sdf;
using Sandbox.Utility;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	private void CreateCavernTerrain()
	{
		var worldLength = GrubsConfig.TerrainLength;
		var worldHeight = GrubsConfig.TerrainHeight;
		var random = Game.Random.Int( RandomMax );

		if ( SeedOverride != null )
			random = SeedOverride.Value;

		// Create base with wavy edges instead of perfect rectangle
		CreateOrganicCavernBase( worldLength, worldHeight, random );

		// Generate pockets (cavities)
		var pockets = GenerateCavernPockets( worldLength, worldHeight, random );

		// Connect pockets with tunnels
		var tunnels = GenerateCavernTunnels( pockets, random );

		// Subtract all pockets and tunnels with organic shapes
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		SubtractCavernShapes( pockets, tunnels, materials.ElementAt( 0 ).Key, random );
	}

	private void CreateOrganicCavernBase( int worldLength, int worldHeight, int seed )
	{
		var cfg = new MaterialsConfig( true, true );
		var materials = GetActiveMaterials( cfg );
		
		// Create main rectangular base
		var boxSdf = new RectSdf( 
			new Vector2( -worldLength / 2f, 0 ), 
			new Vector2( worldLength / 2f, worldHeight )
		);
		
		Add( SdfWorld, boxSdf, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, boxSdf, RockMaterial );
		
		// Add VERY wavy top surface using heightmap with increased amplitude
		var freq = GrubsConfig.TerrainFrequency; // Use full frequency for waviness
		var heightMapSdf = new HeightmapSdf2D( 
			new Vector2( -worldLength / 2f, worldHeight * 0.5f ), // Start lower for bigger waves
			new Vector2( worldLength / 2f, worldHeight ),
			freq,
			seed );
		
		Add( SdfWorld, heightMapSdf, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, heightMapSdf, RockMaterial );
		
		// Add another heightmap layer for extra variation
		var heightMapSdf2 = new HeightmapSdf2D( 
			new Vector2( -worldLength / 2f, worldHeight * 0.6f ), 
			new Vector2( worldLength / 2f, worldHeight ),
			freq * 1.5f, // Different frequency for complexity
			seed + 500 );
		
		Add( SdfWorld, heightMapSdf2, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, heightMapSdf2, RockMaterial );
	}

	private List<CavernPocket> GenerateCavernPockets( int worldLength, int worldHeight, int seed )
	{
		var pockets = new List<CavernPocket>();
		var random = new Random( seed );

		var minRadius = GrubsConfig.TerrainCavernPocketMinSize;
		var maxRadius = GrubsConfig.TerrainCavernPocketMaxSize;
		var edgeMargin = 200f; // Increased margin to account for wavy edges
		var minPocketDistance = 100f;

		var pocketCount = GrubsConfig.TerrainCavernPockets;

		for ( int i = 0; i < pocketCount; i++ )
		{
			var attempts = 0;
			var maxAttempts = 50;
			bool validPosition = false;

			while ( !validPosition && attempts < maxAttempts )
			{
				attempts++;

				var x = random.Next( 
					(int)(-worldLength / 2f + edgeMargin), 
					(int)(worldLength / 2f - edgeMargin) 
				);
				var y = random.Next( 
					(int)(edgeMargin), 
					(int)(worldHeight - edgeMargin) 
				);
				var radius = (float)random.NextDouble() * (maxRadius - minRadius) + minRadius;

				var position = new Vector2( x, y );

				validPosition = true;
				foreach ( var existing in pockets )
				{
					var distance = Vector2.DistanceBetween( position, existing.Position );
					if ( distance < (radius + existing.Radius + minPocketDistance) )
					{
						validPosition = false;
						break;
					}
				}

				if ( validPosition )
				{
					pockets.Add( new CavernPocket 
					{ 
						Position = position, 
						Radius = radius 
					} );
				}
			}
		}

		return pockets;
	}

	private List<CavernTunnel> GenerateCavernTunnels( List<CavernPocket> pockets, int seed )
	{
		var tunnels = new List<CavernTunnel>();
		var random = new Random( seed + 1 );

		if ( pockets.Count < 2 )
			return tunnels;

		// Maximum distance for connections (avoid cross-screen tunnels)
		var maxConnectionDistance = GrubsConfig.TerrainLength/2f;

		for ( int i = 0; i < pockets.Count; i++ )
		{
			var pocket = pockets[i];
			var connectionCount = random.Next( 1, 3 ); // 1-2 connections per pocket
			
			// Find ONLY the closest pockets within max distance
			var nearbyPockets = pockets
				.Where( p => p.Position != pocket.Position )
				.Where( p => Vector2.DistanceBetween( pocket.Position, p.Position ) <= maxConnectionDistance )
				.OrderBy( p => Vector2.DistanceBetween( pocket.Position, p.Position ) )
				.Take( 2 ) // Only consider the 2 closest
				.ToList();

			if ( nearbyPockets.Count == 0 )
				continue;

			var connectionsAdded = 0;
			foreach ( var target in nearbyPockets )
			{
				if ( connectionsAdded >= connectionCount )
					break;

				var tunnelExists = tunnels.Any( t => 
					(t.Start == pocket.Position && t.End == target.Position) ||
					(t.Start == target.Position && t.End == pocket.Position)
				);

				if ( !tunnelExists )
				{
					var tunnelRadius = (float)random.NextDouble() * 30f + 40f;
					tunnels.Add( new CavernTunnel
					{
						Start = pocket.Position,
						End = target.Position,
						Radius = tunnelRadius
					} );
					connectionsAdded++;
				}
			}
		}

		return tunnels;
	}

	private void SubtractCavernShapes( List<CavernPocket> pockets, List<CavernTunnel> tunnels, Sdf2DLayer material, int seed )
	{
		var random = new Random( seed + 2 );

		// Subtract all pockets with organic, irregular shapes
		foreach ( var pocket in pockets )
		{
			// Create highly organic pockets using many overlapping circles with noise-based variation
			var blobCount = random.Next( 8, 15 ); // More circles for smoother shapes
			
			for ( int i = 0; i < blobCount; i++ )
			{
				var angle = (i / (float)blobCount) * MathF.PI * 2f;
				
				// Use noise to vary the distance and size
				var noiseValue = Noise.Perlin( pocket.Position.x * 0.01f + angle, pocket.Position.y * 0.01f );
				var radiusVariation = 0.3f + noiseValue * 0.4f; // 0.3 to 0.7
				var distanceVariation = 0.2f + noiseValue * 0.3f; // 0.2 to 0.5
				
				var offset = new Vector2(
					MathF.Cos( angle ) * pocket.Radius * distanceVariation,
					MathF.Sin( angle ) * pocket.Radius * distanceVariation
				);
				
				var blobRadius = pocket.Radius * radiusVariation;
				var blobCircle = new CircleSdf( pocket.Position + offset, blobRadius );
				Subtract( SdfWorld, blobCircle, material );
			}
			
			// Add the main circle
			var mainCircle = new CircleSdf( pocket.Position, pocket.Radius * 0.6f );
			Subtract( SdfWorld, mainCircle, material );
		}

		// Subtract all tunnels with smooth, winding curves
		foreach ( var tunnel in tunnels )
		{
			SubtractOrganicTunnel( tunnel, material, random );
		}
	}

	private void SubtractOrganicTunnel( CavernTunnel tunnel, Sdf2DLayer material, Random random )
	{
		// Create a metaball-like connection using dual opposing bezier curves
		var start = tunnel.Start;
		var end = tunnel.End;
		var baseRadius = tunnel.Radius * 2f;
		
		// Calculate perpendicular direction for control points
		var direction = (end - start).Normal;
		var perpendicular = new Vector2( -direction.y, direction.x );
		
		// Random curve intensity (scale based on distance for natural proportions)
		var distance = Vector2.DistanceBetween( start, end );
		var curveIntensity = distance * 0.3f + (float)random.NextDouble() * 50f;
		
		var midpoint = (start + end) * 0.5f;
		
		// Create TWO control points on opposite sides for metaball-like pinch
		var controlPoint1 = midpoint + perpendicular * curveIntensity;
		var controlPoint2 = midpoint - perpendicular * curveIntensity;
		
		// Generate BOTH curves
		var segments = 30;
		var curve1Points = new List<Vector2>();
		var curve2Points = new List<Vector2>();
		
		for ( int i = 0; i <= segments; i++ )
		{
			var t = i / (float)segments;
			
			// First curve (top/right)
			var point1 = QuadraticBezier( start, controlPoint1, end, t );
			// Add slight noise variation
			var noiseOffset1 = new Vector2(
				Noise.Perlin( point1.x * 0.05f, point1.y * 0.05f ) * 15f,
				Noise.Perlin( point1.x * 0.05f + 100f, point1.y * 0.05f + 100f ) * 15f
			);
			curve1Points.Add( point1 + noiseOffset1 );
			
			// Second curve (bottom/left)
			var point2 = QuadraticBezier( start, controlPoint2, end, t );
			// Add slight noise variation (different offset for variation)
			var noiseOffset2 = new Vector2(
				Noise.Perlin( point2.x * 0.05f + 200f, point2.y * 0.05f + 200f ) * 15f,
				Noise.Perlin( point2.x * 0.05f + 300f, point2.y * 0.05f + 300f ) * 15f
			);
			curve2Points.Add( point2 + noiseOffset2 );
		}
		
		// Draw the first curve
		DrawCurvePath( curve1Points, baseRadius, material );
		
		// Draw the second curve
		DrawCurvePath( curve2Points, baseRadius, material );
		
		// Fill the space between the curves with additional circles for smooth metaball blending
		for ( int i = 0; i < segments; i += 2 ) // Every other segment to avoid overdoing it
		{
			var t = i / (float)segments;
			
			// Interpolate between the two curves
			var point = (curve1Points[i] + curve2Points[i]) * 0.5f;
			
			// Radius varies - SMALLER in the middle for the "pinch" effect (FLIPPED!)
			var radiusVariation = 0.8f + MathF.Abs( t - 0.5f ) * 0.8f; // Smaller at t=0.5, larger at ends
			var radius = baseRadius * radiusVariation;
			
			var fillCircle = new CircleSdf( point, radius );
			Subtract( SdfWorld, fillCircle, material );
		}
		
		// Add connecting circles at start and end for smooth transition
		var startCircle = new CircleSdf( start, baseRadius * 1.2f );
		Subtract( SdfWorld, startCircle, material );
		
		var endCircle = new CircleSdf( end, baseRadius * 1.2f );
		Subtract( SdfWorld, endCircle, material );
	}

	private void DrawCurvePath( List<Vector2> points, float baseRadius, Sdf2DLayer material )
	{
		for ( int i = 0; i < points.Count - 1; i++ )
		{
			var currentPoint = points[i];
			var nextPoint = points[i + 1];
			
			// Vary radius along the path - PINCH in the middle (FLIPPED!)
			var t = i / (float)(points.Count - 1);
			var radiusVariation =  -MathF.Sin( t * MathF.PI ) * 2f; // Smaller in middle, larger at ends
			var currentRadius = baseRadius * radiusVariation;
			
			// Draw line segment
			var lineSdf = new LineSdf( currentPoint, nextPoint, currentRadius );
			Subtract( SdfWorld, lineSdf, material );
			
			// Add overlapping circles for smoother connections
			var circleSdf = new CircleSdf( currentPoint, currentRadius * 1.15f );
			Subtract( SdfWorld, circleSdf, material );
		}
	}

	private Vector2 QuadraticBezier( Vector2 p0, Vector2 p1, Vector2 p2, float t )
	{
		var u = 1f - t;
		var tt = t * t;
		var uu = u * u;
		
		var point = uu * p0; // (1-t)^2 * P0
		point += 2f * u * t * p1; // 2(1-t)t * P1
		point += tt * p2; // t^2 * P2
		
		return point;
	}
}

/// <summary>
/// Represents a pocket/cavity in the cavern.
/// </summary>
public struct CavernPocket
{
	public Vector2 Position { get; set; }
	public float Radius { get; set; }
}

/// <summary>
/// Represents a tunnel connecting two pockets.
/// </summary>
public struct CavernTunnel
{
	public Vector2 Start { get; set; }
	public Vector2 End { get; set; }
	public float Radius { get; set; }
}
