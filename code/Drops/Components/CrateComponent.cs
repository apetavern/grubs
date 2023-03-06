namespace Grubs;

[Prefab]
public partial class CrateComponent : DropComponent
{
	[Prefab]
	public CrateType CrateType { get; set; }

	public override void Simulate( IClient client )
	{
		var shouldRotate = true;

		var move = new MoveHelper( Drop.Position, Drop.Velocity );
		move.Trace = move.Trace
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
			shouldRotate = false;

			if ( Game.IsServer )
				chute.DeleteAsync( 0.3f );
		}

		move.ApplyFriction( 2.0f, Time.Delta );
		move.TryMove( Time.Delta );

		Drop.Position = move.Position;
		Drop.Velocity = move.Velocity;

		Drop.Rotation = Rotation.Slerp(
			Drop.Rotation,
			Rotation.Identity * new Angles(
				shouldRotate
					? MathF.Sin( Time.Now * 2f ) * 15f
					: 0f, 0, 0 ).ToRotation(),
			0.75f );
	}

	public override void OnTouch( Entity other )
	{
		if ( other is not Grub grub )
			return;

		switch ( CrateType )
		{
			case CrateType.Weapons:
				var weaponResourcePath = CrateDrops.GetRandomWeaponFromCrate();
				Log.Info( weaponResourcePath );
				grub.Player.Inventory.AddByResourcePath( weaponResourcePath );
				Drop.Delete();
				break;
			case CrateType.Tools:
				Drop.Delete();
				break;
			case CrateType.Health:
				grub.Health += 25;
				HealGrubEventClient( To.Everyone, grub, 25 );
				Drop.Delete();
				break;
			default:
				return;
		}
	}

	[ClientRpc]
	public void HealGrubEventClient( Grub grub, int healAmount )
	{
		Event.Run( "grub.healed", grub.NetworkIdent, healAmount );
	}
}

public enum CrateType
{
	Weapons,
	Tools,
	Health
}
