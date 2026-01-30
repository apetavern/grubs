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
		
		// Create main rectangular base with SMOOTHING via Expand
		var boxSdf = new RectSdf( 
			new Vector2( -worldLength / 2f, 0 ), 
			new Vector2( worldLength / 2f, worldHeight*0.5f )
		).Expand( 2f ); // This smooths the corners!
		
		Add( SdfWorld, boxSdf, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, boxSdf, RockMaterial );
		
		// Add VERY wavy top surface using heightmap with increased amplitude
		var freq = 0.01f;
		var heightMapSdf = new HeightmapSdf2D( 
			new Vector2( -worldLength / 2f, 0f ),
			new Vector2( worldLength / 2f, worldHeight ),
			freq,
			seed );

		var transformedHeightMapSdf = heightMapSdf.Transform( new Vector2( 0, worldHeight * 0.5f ) );
		
		// Apply expand to smooth the heightmap too
		var smoothedHeightMap = transformedHeightMapSdf.Expand( 3f );
		
		Add( SdfWorld, smoothedHeightMap, materials.ElementAt( 0 ).Key );
		Add( SdfWorld, smoothedHeightMap, RockMaterial );
	}

	private List<CavernPocket> GenerateCavernPockets( int worldLength, int worldHeight, int seed )
	{
		var pockets = new List<CavernPocket>();
		var random = new Random( seed );

		var minRadius = GrubsConfig.TerrainCavernPocketMinSize;
		var maxRadius = GrubsConfig.TerrainCavernPocketMaxSize;
		var edgeMargin = 200f;
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

		var maxConnectionDistance = GrubsConfig.TerrainLength/2f;

		for ( int i = 0; i < pockets.Count; i++ )
		{
			var pocket = pockets[i];
			var connectionCount = random.Next( 1, 3 );
			
			var nearbyPockets = pockets
				.Where( p => p.Position != pocket.Position )
				.Where( p => Vector2.DistanceBetween( pocket.Position, p.Position ) <= maxConnectionDistance )
				.OrderBy( p => Vector2.DistanceBetween( pocket.Position, p.Position ) )
				.Take( 2 )
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
					tunnels.Add( new CavernTunnel
					{
						Start = pocket.Position,
						End = target.Position,
						StartRadius = pocket.Radius, // Store the source pocket radius
						EndRadius = target.Radius    // Store the target pocket radius
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
			// Increase blob count for smoother blending
			var blobCount = random.Next( 12, 20 ); // More circles = smoother
			
			for ( int i = 0; i < blobCount; i++ )
			{
				var angle = (i / (float)blobCount) * MathF.PI * 2f;
				
				var noiseValue = Noise.Perlin( pocket.Position.x * 0.01f + angle, pocket.Position.y * 0.01f );
				var radiusVariation = 0.5f + noiseValue * 0.4f;
				var distanceVariation = 0.2f + noiseValue * 0.3f;
				
				var offset = new Vector2(
					MathF.Cos( angle ) * pocket.Radius * distanceVariation,
					MathF.Sin( angle ) * pocket.Radius * distanceVariation
				);
				
				var blobRadius = pocket.Radius * radiusVariation;
				// Apply Expand to each circle for smoothing
				var blobCircle = new CircleSdf( pocket.Position + offset, blobRadius ).Expand( 2f );
				Subtract( SdfWorld, blobCircle, material );
			}
			
			// Main circle also expanded for smoothing
			var mainCircle = new CircleSdf( pocket.Position, pocket.Radius * 0.6f ).Expand( 3f );
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
		var start = tunnel.Start;
		var end = tunnel.End;
		
		// Use the pocket radii for start and end sizes
		var startRadius = tunnel.StartRadius;
		var endRadius = tunnel.EndRadius;
		
		var direction = (end - start).Normal;
		var perpendicular = new Vector2( -direction.y, direction.x );
		
		var distance = Vector2.DistanceBetween( start, end );
		var curveIntensity = distance * 0.3f + (float)random.NextDouble() * 50f;
		
		var midpoint = (start + end) * 0.5f;
		
		var controlPoint1 = midpoint + perpendicular * curveIntensity;
		var controlPoint2 = midpoint - perpendicular * curveIntensity;
		
		var segments = 40; // More segments for smoother transitions
		var curve1Points = new List<Vector2>();
		var curve2Points = new List<Vector2>();
		
		for ( int i = 0; i <= segments; i++ )
		{
			var t = i / (float)segments;
			
			var point1 = QuadraticBezier( start, controlPoint1, end, t );
			var noiseOffset1 = new Vector2(
				Noise.Perlin( point1.x * 0.05f, point1.y * 0.05f ) * 15f,
				Noise.Perlin( point1.x * 0.05f + 100f, point1.y * 0.05f + 100f ) * 15f
			);
			curve1Points.Add( point1 + noiseOffset1 );
			
			var point2 = QuadraticBezier( start, controlPoint2, end, t );
			var noiseOffset2 = new Vector2(
				Noise.Perlin( point2.x * 0.05f + 200f, point2.y * 0.05f + 200f ) * 15f,
				Noise.Perlin( point2.x * 0.05f + 300f, point2.y * 0.05f + 300f ) * 15f
			);
			curve2Points.Add( point2 + noiseOffset2 );
		}
		
		// Draw both curves with interpolated radii
		DrawCurvePath( curve1Points, startRadius, endRadius, material );
		DrawCurvePath( curve2Points, startRadius, endRadius, material );
		
		// Fill between curves with interpolated radius
		for ( int i = 0; i <= segments; i += 2 )
		{
			var t = i / (float)segments;
			var point = (curve1Points[i] + curve2Points[i]) * 0.5f;
			
			// Interpolate between start and end radius
			var baseRadius = MathX.Lerp( startRadius, endRadius, t );
			
			// Apply squeeze in the middle (stronger squeeze)
			var squeezeAmount = MathF.Sin( t * MathF.PI ); // 0 at ends, 1 in middle
			var squeezeFactor = 1f - squeezeAmount * 0.5f; // Squeeze to 50% at midpoint
			
			var radius = baseRadius * squeezeFactor;
			
			var fillCircle = new CircleSdf( point, radius ).Expand( 3f );
			Subtract( SdfWorld, fillCircle, material );
		}
		
		// Smooth connection circles at full pocket sizes
		var startCircle = new CircleSdf( start, startRadius ).Expand( 4f );
		Subtract( SdfWorld, startCircle, material );
		
		var endCircle = new CircleSdf( end, endRadius ).Expand( 4f );
		Subtract( SdfWorld, endCircle, material );
	}

	private void DrawCurvePath( List<Vector2> points, float startRadius, float endRadius, Sdf2DLayer material )
	{
		for ( int i = 0; i < points.Count - 1; i++ )
		{
			var currentPoint = points[i];
			var nextPoint = points[i + 1];
			
			// Interpolate between start and end radius
			var t = i / (float)(points.Count - 1);
			var baseRadius = MathX.Lerp( startRadius, endRadius, t );
			
			// Apply squeeze in the middle
			var squeezeAmount = MathF.Sin( t * MathF.PI ); // Peak at t=0.5
			var squeezeFactor = - squeezeAmount * 0.5f; // Squeeze to 50%
			
			var currentRadius = baseRadius * squeezeFactor;
			
			// Apply Expand to line segments for smoothing
			var lineSdf = new LineSdf( currentPoint, nextPoint, currentRadius ).Expand( 3f );
			Subtract( SdfWorld, lineSdf, material );
			
			// Smooth overlapping circles
			var circleSdf = new CircleSdf( currentPoint, currentRadius * 1.1f ).Expand( 3f );
			Subtract( SdfWorld, circleSdf, material );
		}
	}

	private Vector2 QuadraticBezier( Vector2 p0, Vector2 p1, Vector2 p2, float t )
	{
		var u = 1f - t;
		var tt = t * t;
		var uu = u * u;
		
		var point = uu * p0;
		point += 2f * u * t * p1;
		point += tt * p2;
		
		return point;
	}
}

public struct CavernPocket
{
	public Vector2 Position { get; set; }
	public float Radius { get; set; }
}

public struct CavernTunnel
{
	public Vector2 Start { get; set; }
	public Vector2 End { get; set; }
	public float StartRadius { get; set; } // Radius of the starting pocket
	public float EndRadius { get; set; }   // Radius of the ending pocket
}
