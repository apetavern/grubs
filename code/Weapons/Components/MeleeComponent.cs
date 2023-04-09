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

	[Prefab, ResourceType( "sound" )]
	public string HitSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string ImpactSound { get; set; }

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring && TimeSinceFired > HitDelay )
			Fire();
	}

	public override void FireInstant()
	{
		var grubsHit = GetGrubsInSwing();

		if ( Game.IsServer )
		{
			foreach ( var (grub, dir) in grubsHit )
			{
				grub.ApplyAbsoluteImpulse( HitForce * dir );
				grub.TakeDamage( new DamageInfo
				{
					Attacker = Grub,
					Damage = Damage,
					Position = grub.Position,
				}.WithTag( "melee" ) );
			}
		}

		if ( grubsHit is not null && grubsHit.Count > 0 )
			Weapon.PlaySound( ImpactSound );

		Grub.SetAnimParameter( "fire", true );
		Weapon.PlaySound( HitSound );

		FireFinished();
	}

	private Dictionary<Grub, Vector3> GetGrubsInSwing()
	{
		if ( Game.IsClient )
			return null;

		var ray = new Ray( Grub.EyePosition + HitOffset, Grub.Facing * Grub.EyeRotation.Forward );
		var trs = Trace.Ray( ray, HitSize.x ).Ignore( Grub ).WithoutTags( "dead" ).RunAll();

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
