namespace Grubs;

public partial class ExplosiveComponent : EntityComponent<Explosive>
{
	protected Explosive Explosive => Entity;
	protected Grub Grub => Explosive.Grub;
	protected Player Player => Grub.Player;

	private bool HasStarted = false;

	public virtual void OnStart()
	{

	}

	public virtual void Simulate( IClient client )
	{
		if ( !HasStarted )
		{
			OnStart();
			HasStarted = true;
		}

		ExplodeAfterSeconds( Explosive.ExplodeAfter );
	}

	public virtual void Explode()
	{
		Explosive.Delete();
	}

	// TODO: Remove or move, just for debugging.
	public async void ExplodeAfterSeconds( float seconds )
	{
		await GameTask.DelaySeconds( seconds );

		if ( !Explosive.IsValid() )
			return;

		Explode();
	}
}
