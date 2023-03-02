namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	public virtual void Simulate( IClient client )
	{
		PhysicsTick();
	}

	protected virtual void PhysicsTick()
	{
		// Apply gravity.
		Explosive.Velocity -= new Vector3( 0, 0, 400 ) * Time.Delta;

		var helper = new MoveHelper( Explosive.Position, Explosive.Velocity );
		helper.Trace = helper.Trace.Size( 12f ).WithAnyTags( "player", "solid" ).WithoutTags( "dead" ).Ignore( Explosive );
		helper.TryMove( Time.Delta );
		Explosive.Velocity = helper.Velocity;
		Explosive.Position = helper.Position;

		if ( Explosive.ShouldRotate )
		{
			// Apply rotation using some shit I pulled out of my ass.
			var angularX = Explosive.Velocity.x * 5f * Time.Delta;
			float degrees = angularX.Clamp( -20, 20 );
			Explosive.Rotation = Explosive.Rotation.RotateAroundAxis( new Vector3( 0, 1, 0 ), degrees );
		}
		else
		{
			Explosive.Rotation = Rotation.Identity;
		}
	}

	protected virtual void Explode()
	{
		switch ( Explosive.ExplosionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Explosive.Position, Grub, Explosive.ExplosionRadius, Explosive.MaxExplosionDamage );
				break;
			case ExplosiveReaction.Incendiary:
				// FireHelper.StartFiresAt( Position, Segments[Segments.Count - 1].EndPos - Segments[Segments.Count - 1].StartPos, 10 );
				break;
		}

		ExplodeSoundClient( To.Everyone, Explosive.ExplosionSound );
		Explosive.Delete();
	}

	[ClientRpc]
	public void ExplodeSoundClient( string explosionSound )
	{
		Explosive.SoundFromScreen( explosionSound );
	}
}
