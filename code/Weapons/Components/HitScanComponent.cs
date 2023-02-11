using Sandbox.Csg;

namespace Grubs;

[Prefab]
public partial class HitScanComponent : WeaponComponent
{
	[Prefab, Net]
	public Vector3 HitForce { get; set; }

	[Prefab, Net]
	public int TraceCount { get; set; } = 1;

	[Prefab, Net]
	public float TraceSpread { get; set; } = 1f;

	[Prefab, Net]
	public float TraceDelay { get; set; } = 0f;

	[Prefab, Net]
	public float TraceDistance { get; set; } = 0f;

	[Prefab, Net]
	public float ExplosionRadius { get; set; } = 0f;

	[Prefab, Net]
	public float ExplosionDamage { get; set; } = 1f;

	[Prefab, Net]
	public bool PenetrateTargets { get; set; } = false;

	[Prefab, Net]
	public bool PenetrateWorld { get; set; } = false;

	[Prefab, Net]
	public float Damage { get; set; } = 25;

	[Prefab]
	public bool UseMuzzleParticle { get; set; } = false;

	[Prefab, Net]
	public ParticleSystem TraceParticle { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceLastHitScan { get; set; }

	[Net, Predicted]
	public int FireCount { get; set; } = 0;

	public static readonly string MuzzleParticlesPath = "particles/muzzleflash/grubs_muzzleflash.vpcf";

	public override bool ShouldStart()
	{
		return Grub.IsTurn && Grub.Controller.IsGrounded;
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring && TimeSinceLastHitScan >= TraceDelay && FireCount < TraceCount )
		{
			Fire();
		}
	}

	public override void FireInstant()
	{
		// Setup start and end positions from muzzle if possible.
		// Otherwise, use Weapon position and Grub.EyeRotation.
		var muzzle = Weapon.GetAttachment( "muzzle" );
		var startPos = muzzle is not null ? muzzle.Value.Position : Weapon.Position;
		var endPos = muzzle is not null
			? startPos + muzzle.Value.Rotation.Forward * TraceDistance + (Vector3.Random * TraceSpread)
			: startPos + Grub.EyeRotation.Forward * TraceDistance + (Vector3.Random * TraceSpread);
		startPos = startPos.WithY( 0f );
		endPos = endPos.WithY( 1f );

		// Handle muzzle flash if applicable.
		if ( UseMuzzleParticle && muzzle is not null )
		{
			var muzzleFlash = Particles.Create( MuzzleParticlesPath, muzzle.Value.Position );
			muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
		}

		// Trace the shot.
		var tr = Trace.Ray( startPos, endPos ).Ignore( Grub );


		if ( Game.IsServer )
		{
			Vector3 particleEndPosition = endPos;

			if ( PenetrateWorld )
			{
				tr = tr.WithoutTags( "solid" );
				GrubsGame.Instance.World.SubtractLine( startPos, endPos, ExplosionRadius );
			}

			TraceResult[] result;
			if ( PenetrateTargets )
			{
				result = tr.RunAll();
				if ( result is not null )
				{
					TraceHitMultiple( result );
				}
			}
			else
			{
				result = new TraceResult[1];
				result[0] = tr.Run();
				particleEndPosition = result[0].EndPosition;
				TraceHitSingle( result[0] );
			}

			var traceParticles = Particles.Create( TraceParticle.ResourcePath );
			traceParticles?.SetPosition( 0, startPos );
			traceParticles?.SetPosition( 1, particleEndPosition );
		}

		if ( TraceDelay > 0 )
		{
			FireEffects();
		}

		FireCount++;
		TimeSinceLastHitScan = 0;

		if ( FireCount >= TraceCount )
		{
			IsFiring = false;
			FireCount = 0;

			if ( TraceDelay == 0 )
			{
				FireEffects();
			}
		}
	}

	private void FireEffects()
	{
		Grub.SetAnimParameter( "fire", true );
	}

	private void TraceHitSingle( TraceResult tr )
	{
		if ( tr.Hit )
		{
			if ( tr.Entity is Grub grub )
			{
				HitGrub( grub, tr.Direction );
			}
			else if ( tr.Entity is CsgSolid )
			{
				ExplosionHelper.Explode( tr.EndPosition, Grub, ExplosionRadius, ExplosionDamage );
			}
		}
	}

	private void TraceHitMultiple( TraceResult[] results )
	{
		for ( int i = 0; i < results.Length; i++ )
		{
			TraceHitSingle( results[i] );
		}
	}

	private void HitGrub( Grub grub, Vector3 direction )
	{
		grub.ApplyAbsoluteImpulse( HitForce * direction );
		grub.TakeDamage( new DamageInfo
		{
			Attacker = Grub,
			Damage = Damage,
			Position = grub.Position,
		} );
	}
}
