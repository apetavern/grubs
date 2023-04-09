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

	[Prefab, ResourceType( "sound" )]
	public string TraceSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string StartupSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string FinishSound { get; set; }

	[Prefab, Net]
	public float ExplosionRadius { get; set; } = 0f;

	[Prefab, Net]
	public float ExplosionDamage { get; set; } = 1f;

	[Prefab, Net]
	public bool PenetrateTargets { get; set; } = false;

	[Prefab, Net]
	public bool PenetrateWorld { get; set; } = false;

	[Prefab, Net]
	public bool AutoMove { get; set; } = false;

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

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring && TimeSinceLastHitScan >= TraceDelay && FireCount < TraceCount )
			Fire();

		if ( IsFiring && AutoMove )
			Grub.MoveInput = -Grub.Facing * 0.75f;
	}

	public override void FireInstant()
	{
		// Setup start and end positions from muzzle if possible.
		// Otherwise, use Weapon position and Grub.EyeRotation.
		var muzzle = Weapon.GetAttachment( "muzzle" );
		var startPos = Weapon.GetStartPosition();
		var endPos = muzzle is not null
			? startPos + muzzle.Value.Rotation.Forward * TraceDistance + (Vector3.Random * TraceSpread)
			: startPos + Grub.EyeRotation.Forward * TraceDistance + (Vector3.Random * TraceSpread);
		var pitch = muzzle is not null ? muzzle.Value.Rotation.Pitch() : Grub.EyeRotation.Pitch();
		pitch *= Grub.Facing;
		var rotation = Rotation.FromPitch( pitch );
		startPos = startPos.WithY( 0f );
		endPos = endPos.WithY( 1f );

		// Handle muzzle flash if applicable.
		if ( UseMuzzleParticle && muzzle is not null )
		{
			var muzzleFlash = Particles.Create( MuzzleParticlesPath, muzzle.Value.Position );
			muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
		}

		// Trace the shot.
		var tr = Trace.Ray( startPos, endPos ).WithoutTags( "dead" ).Ignore( Grub );

		if ( Game.IsServer )
		{
			Vector3 particleEndPosition = endPos;

			if ( PenetrateWorld )
			{
				tr = tr.WithoutTags( "solid" );
				GamemodeSystem.Instance.GameWorld.SubtractLine( startPos, endPos, ExplosionRadius, rotation );
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
			FireCount = 0;

			if ( TraceDelay == 0 )
				FireEffects();

			FireFinished();
		}
	}

	public override void FireFinished()
	{
		base.FireFinished();

		Weapon.PlaySound( FinishSound );
	}

	private void FireEffects()
	{
		Grub.SetAnimParameter( "fire", true );
		Weapon.PlaySound( TraceSound );
	}

	private bool TraceHitSingle( TraceResult tr )
	{
		if ( tr.Hit )
		{
			if ( tr.Entity is Grub grub )
			{
				HitGrub( grub, -tr.Normal );
				return false;
			}
			else if ( tr.Entity is CsgSolid )
			{
				ExplosionHelper.Explode( tr.EndPosition, Grub, ExplosionRadius, ExplosionDamage );
				return true;
			}
		}

		return false;
	}

	private void TraceHitMultiple( TraceResult[] results )
	{
		for ( int i = 0; i < results.Length; i++ )
		{
			var hitTerrain = TraceHitSingle( results[i] );
			if ( hitTerrain )
				return;
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
		}.WithTag( "hitscan" ) );
	}
}
