namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	public virtual void Simulate( IClient client )
	{

	}

	public virtual void Explode()
	{
		switch ( Explosive.ExplosionReaction )
		{
			case ExplosiveReaction.Explosion:
				ExplosionHelper.Explode( Explosive.Position, Grub, Explosive.ExplosionRadius );
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
