using Sandbox.Csg;
using System.Numerics;

namespace Grubs;

public partial class World
{
	public void SubtractDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( CoolBrush, (min + max) * 0.5f, max - min );
		PaintDefault( min, max );
	}

	public void SubtractDefault( CsgBrush brush, Vector3 min, Vector3 max )
	{
		CsgWorld.Subtract( brush, (min + max) * 0.5f, max - min );
		PaintDefault( min, max );
	}

	public void SubtractBackground( Vector3 min, Vector3 max )
	{
		CsgBackground.Subtract( CoolBrush, (min + max) * 0.5f, max - min );
	}

	public void SubtractBackground( CsgBrush brush, Vector3 min, Vector3 max )
	{
		CsgBackground.Subtract( brush, (min + max) * 0.5f, max - min );
	}

	public void SubtractLine( Vector3 start, Vector3 stop, float size, Rotation rotation )
	{
		var midpoint = new Vector3( (start.x + stop.x) / 2, 0f, (start.z + stop.z) / 2 );
		var scale = new Vector3( Vector3.DistanceBetween( start, stop ), 64f, size );

		CsgWorld.Subtract( CubeBrush, midpoint, scale, Rotation.FromPitch( rotation.Pitch() ) );
		CsgWorld.Paint( CubeBrush, DefaultMaterial, midpoint, scale.WithZ( size * 1.1f ), Rotation.FromPitch( rotation.Pitch() ) );
	}

	public void SubtractCoolLine( Vector3 start, Vector3 stop, float size, Rotation rotation )
	{
		var midpoint = new Vector3( (start.x + stop.x) / 2, 0f, (start.z + stop.z) / 2 );
		var scale = new Vector3( Vector3.DistanceBetween( start, stop ), 64f, size );

		CsgWorld.Subtract( CoolBrush, midpoint, scale, Rotation.FromPitch( rotation.Pitch() ) );
		CsgWorld.Paint( CoolBrush, DefaultMaterial, midpoint, scale.WithZ( size * 1.1f ), Rotation.FromPitch( rotation.Pitch() ) );
	}

	public void AddDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Add( CoolBrush, SandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}

	public void AddDefaultCube( Vector3 min, Vector3 max )
	{
		CsgWorld.Add( CubeBrush, SandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}

	public void AddLine( Vector3 start, Vector3 stop, float size, Rotation rotation, bool Caps = true, CsgMaterial mat = null )
	{
		var midpoint = new Vector3( (start.x + stop.x) / 2, 0f, (start.z + stop.z) / 2 );
		var scale = new Vector3( Vector3.DistanceBetween( start, stop ), 64f, size );

		CsgWorld.Add( CubeBrush, mat == null ? SandMaterial : mat, midpoint, scale, Rotation.FromPitch( rotation.Pitch() ) );

		if ( Caps )
		{
			CsgWorld.Add( CoolBrush, mat == null ? SandMaterial : mat, start, (Vector3.One * size).WithY( 64f ) );
			CsgWorld.Add( CoolBrush, mat == null ? SandMaterial : mat, stop, (Vector3.One * size).WithY( 64f ) );
		}
		//CsgWorld.Paint( CubeBrush, DefaultMaterial, midpoint, scale.WithZ( size * 1.1f ), Rotation.FromPitch( rotation.Pitch() ) );
	}

	public void AddTextureStamp( string TexturePath, Vector3 Center, float Angle )
	{
		ResourceLibrary.TryGet( TexturePath, out TextureStamp stamp );
		if ( stamp != null )
		{
			float resolution = 8;

			var pointsX = stamp.texture.Width;
			var pointsY = stamp.texture.Height;

			Color32[] pixels = stamp.texture.GetPixels().Reverse().ToArray();

			List<Vector3> points = new List<Vector3>();
			List<Vector3> lineStarts = new List<Vector3>();
			List<Vector3> lineEnds = new List<Vector3>();

			float lineWidth = 14.0f; // width of each CSG line

			Rotation RotationAngle = new Angles( Angle, 0, 0 ).ToRotation();

			for ( int i = 0; i < pixels.Length; i++ )
			{
				int x = i % pointsX;
				int y = i / pointsX;

				// check if the current pixel is part of a CSG line
				if ( pixels[i].a > 0 )
				{
					var min = new Vector3( (x * resolution) - resolution, -8, (y * resolution) - resolution );
					var max = new Vector3( (x * resolution) + resolution, 8, (y * resolution) + resolution );

					min *= RotationAngle;
					max *= RotationAngle;

					// Offset by position.
					min += Center.WithY( 0 );
					max += Center.WithY( 0 );

					// add the current point to the list of points
					points.Add( (min + max) / 2 );

					// check if the next pixel in the same row is also part of the line
					int nextX = x;
					while ( nextX < pointsX - 1 && pixels[i + 1].a > 0 )
					{
						nextX++;
						i++;
					}
					if ( nextX > x )
					{
						// add a line between the current point and the last point in the same row
						min = new Vector3( (nextX * resolution) - resolution, -16, (y * resolution) - resolution );
						max = new Vector3( (nextX * resolution) + resolution, 16, (y * resolution) + resolution );

						min *= RotationAngle;
						max *= RotationAngle;

						// Offset by position.
						min += Center.WithY( 0 );
						max += Center.WithY( 0 );

						points.Add( (min + max) / 2 );

						lineStarts.Add( points[points.Count - 2] - (RotationAngle.Forward * pointsX * resolution * 0.5f) - (RotationAngle.Up * pointsY * resolution * 0.5f) );
						lineEnds.Add( points[points.Count - 1] - (RotationAngle.Forward * pointsX * resolution * 0.5f) - (RotationAngle.Up * pointsY * resolution * 0.5f) );
					}
				}
			}

			for ( int i = 0; i < lineStarts.Count; i++ )
			{
				Vector3 start = lineStarts[i];
				Vector3 end = lineEnds[i];

				float size = lineWidth;
				Quaternion rotation = RotationAngle;
				AddLine( start, end, size, rotation, false, stamp.material );
			}
		}
	}

	public static Rotation FromAngleAxis( float angle, Vector3 axis )
	{
		axis = axis.Normal;
		float halfAngle = angle * 0.5f;
		float sinHalfAngle = (float)Math.Sin( halfAngle );
		float cosHalfAngle = (float)Math.Cos( halfAngle );
		float x = axis.x * sinHalfAngle;
		float y = axis.y * sinHalfAngle;
		float z = axis.z * sinHalfAngle;
		float w = cosHalfAngle;
		return new Rotation( x, y, z, w );
	}

	public void AddBackgroundLine( Vector3 start, Vector3 stop, float size, Rotation rotation )
	{
		var midpoint = new Vector3( (start.x + stop.x) / 2, 64, (start.z + stop.z) / 2 );
		var scale = new Vector3( Vector3.DistanceBetween( start, stop ), 64f, size );

		CsgBackground.Add( CubeBrush, RockMaterial, midpoint, scale, Rotation.FromPitch( rotation.Pitch() ) );

		//DebugOverlay.Sphere( start, 64f, Color.Red, 1f );

		CsgBackground.Add( CoolBrush, RockMaterial, start, (Vector3.One * size).WithY( 64f ) );
		CsgBackground.Add( CoolBrush, RockMaterial, stop, (Vector3.One * size).WithY( 64f ) );
		//CsgWorld.Paint( CubeBrush, DefaultMaterial, midpoint, scale.WithZ( size * 1.1f ), Rotation.FromPitch( rotation.Pitch() ) );
	}

	public void AddDefaultBackground( Vector3 min, Vector3 max )
	{
		CsgBackground.Add( CoolBrush, RockMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}

	public void PaintDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Paint( CoolBrush, AltSandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}
}
