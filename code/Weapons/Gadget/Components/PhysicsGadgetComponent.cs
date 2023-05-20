namespace Grubs;

[Prefab]
public partial class PhysicsGadgetComponent : GadgetComponent
{
	[Prefab, Net]
	public bool ShouldRotate { get; set; } = true;

	[Prefab, Net]
	public bool ShouldThrow { get; set; } = false;

	[Prefab, Net]
	public int ThrowSpeed { get; set; } = 10;

	[Prefab, Net]
	public bool AffectedByWind { get; set; } = false;

	[Prefab, Net]
	public bool CheckResolve { get; set; } = true;

	[Prefab, ResourceType( "sound" )]
	public string CollisionSound { get; set; }

	public override void OnUse( Weapon weapon, int charge )
	{
		if ( ShouldThrow )
		{
			Gadget.Position = weapon.GetStartPosition();
			Gadget.Velocity = Grub.EyeRotation.Forward.Normal * Grub.Facing * charge * ThrowSpeed;
		}
	}

	public override bool IsResolved()
	{
		return CheckResolve ? Gadget.Velocity.IsNearlyZero( 2.5f ) : true;
	}

	TimeSince collisionSoundPlayed;
	public override void Simulate( IClient client )
	{
		// Apply gravity.
		Gadget.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.WithAnyTags( "player", "solid" )
			.WithoutTags( "dead" );

		var groundEntity = helper.TraceDirection( Vector3.Down ).Entity;

		if ( groundEntity is not null )
			helper.ApplyFriction( 1.0f, Time.Delta );

		helper.TryMove( Time.Delta );
		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;

		if(CollisionSound is not null && collisionSoundPlayed >= .3f)
		{
			if( !Gadget.Velocity.IsNearlyZero( 20f ) && (helper.HitWall || groundEntity is not null))
			{
				Gadget.PlaySound( CollisionSound );
				collisionSoundPlayed = 0f;
			}
		}

		if ( GrubsConfig.WindEnabled && AffectedByWind )
			Gadget.Velocity += new Vector3( GamemodeSystem.Instance.ActiveWindForce ).WithY( 0 );

		if ( ShouldRotate )
		{
			// Apply rotation using some shit I pulled out of my ass.
			var angularX = Gadget.Velocity.x * 5f * Time.Delta;
			float degrees = angularX.Clamp( -20, 20 );
			Gadget.Rotation = Gadget.Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
		}
		else
		{
			Gadget.Rotation = Rotation.Identity;
		}
	}
}
