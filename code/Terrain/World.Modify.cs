using Sandbox.Csg;

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

	public void AddDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Add( CoolBrush, SandMaterial, (min + max) * 0.5f, (max - min) * 1.2f);
	}

	public void PaintDefault( Vector3 min, Vector3 max )
	{
		CsgWorld.Paint( CoolBrush, AltSandMaterial, (min + max) * 0.5f, (max - min) * 1.2f );
	}
}
