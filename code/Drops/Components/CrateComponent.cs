namespace Grubs;

[Prefab]
public class CrateComponent : DropComponent
{
	[Prefab] public CrateType CrateType { get; set; }

	public override void Simulate( IClient client )
	{
		var move = new MoveHelper( Drop.Position, Drop.Velocity );
		move.Trace = move.Trace
			.Size( Drop.Size )
			.Ignore( Drop )
			.WithAnyTags( "player", "solid" );
		var groundEntity = move.TraceDirection( Vector3.Down ).Entity;

		if ( groundEntity is null )
		{
			move.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			move.Velocity = 0;
			
			var parachute = Drop.Children.FirstOrDefault( c => c.Tags.Has( "parachute" ) );
			if ( parachute is not AnimatedEntity chute )
				return;
			
			chute.SetAnimParameter( "landed", true );

			if ( Game.IsServer )
				chute.DeleteAsync( 0.3f );
		}
		
		move.ApplyFriction( 2.0f, Time.Delta );
		move.TryMove( Time.Delta );

		Drop.Position = move.Position;
		Drop.Velocity = move.Velocity;
	}
}

public enum CrateType
{
	Weapons,
	Tools,
	Health
}
