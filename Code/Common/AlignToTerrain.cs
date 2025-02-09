using Sandbox;

namespace Grubs;

public sealed class AlignToTerrain : Component, Component.ICollisionListener
{
	public void OnCollisionStart( Collision collision )
	{
		var tr = Scene.Trace.Ray( WorldPosition, WorldPosition + Vector3.Down * 24f )
			.WithAnyTags( "solid", "player" )
			.Run();

		if ( tr.Hit )
			WorldRotation = Rotation.FromToRotation( Vector3.Up, tr.Normal );
	}
}
