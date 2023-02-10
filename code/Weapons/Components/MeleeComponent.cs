namespace Grubs;

[Prefab]
public partial class MeleeComponent : WeaponComponent
{
	[Prefab, Net]
	public float Damage { get; set; } = 25;

	[Prefab, Net]
	public Vector3 HitForce { get; set; }

	[Prefab]
	public Vector3 HitSize { get; set; }

	[Prefab, Net]
	public Vector3 HitOffset { get; set; }

	[Prefab, Net]
	public float HitDelay { get; set; }

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring && TimeSinceFired > HitDelay )
		{
			Fire();
		}
	}

	public override void FireInstant()
	{
		if ( Game.IsServer )
		{
			var grubsHit = GetGrubsInSwing();
			foreach ( var (grub, dir) in grubsHit )
			{
				grub.ApplyAbsoluteImpulse( HitForce * dir );
			}
		}

		IsFiring = false;
		Grub.SetAnimParameter( "fire", true );
	}

	public override void FireCharged()
	{
		Grub.SetAnimParameter( "fire", true );
	}

	private Dictionary<Grub, Vector3> GetGrubsInSwing()
	{
		if ( Game.IsClient )
			return null;

		var ray = new Ray( Grub.EyePosition + HitOffset, Grub.Facing * Grub.EyeRotation.Forward );
		var trs = Trace.Ray( ray, HitSize.x ).Ignore( Grub ).RunAll();

		var grubsHitToDirection = new Dictionary<Grub, Vector3>();

		if ( trs is null )
			return grubsHitToDirection;

		foreach ( var trace in trs )
		{
			if ( trace.Hit && trace.Entity is Grub grub )
			{
				grubsHitToDirection.Add( grub, trace.Direction );
			}
		}

		return grubsHitToDirection;
	}
}
