using Sandbox.Sdf;

namespace Grubs;

[Prefab]
public partial class FireEmitComponent : WeaponComponent
{
	[Prefab, Net]
	public int FlameVelocity { get; set; } = 1;

	[Prefab, Net]
	public int FlamesCount { get; set; } = 1;

	[Prefab, Net]
	public float FlamesSpread { get; set; } = 1f;

	[Prefab, Net]
	public float FlameDelay { get; set; } = 0f;

	[Prefab, ResourceType( "sound" )]
	public string FlameSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string StartupSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string FinishSound { get; set; }

	[Prefab, Net]
	public bool AutoMove { get; set; } = false;

	[Prefab, Net]
	public ParticleSystem MuzzleParticle { get; set; }

	[Net, Predicted]
	public TimeSince TimeSinceLastFlame { get; set; }

	[Net, Predicted]
	public int FireCount { get; set; } = 0;

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( IsFiring && TimeSinceLastFlame >= FlameDelay && FireCount < FlamesCount )
			Fire();

		if ( IsFiring && AutoMove )
		{
			Grub.MoveInput = -Grub.Facing * 0.75f;

			if ( Input.Down( InputAction.Fire ) )
				FireFinished();
		}
	}

	public override void FireInstant()
	{
		// Setup start and end positions from muzzle if possible.
		// Otherwise, use Weapon position and Grub.EyeRotation.
		var muzzle = Weapon.GetAttachment( "muzzle" );
		var startPos = Weapon.GetStartPosition();
		var endPos = muzzle is not null
			? startPos + muzzle.Value.Rotation.Forward * FlameVelocity + (Vector3.Random * FlamesSpread)
			: startPos + Grub.EyeRotation.Forward * FlameVelocity + (Vector3.Random * FlamesSpread);
		var pitch = muzzle is not null ? muzzle.Value.Rotation.Pitch() : Grub.EyeRotation.Pitch();
		pitch *= Grub.Facing;
		startPos = startPos.WithY( 0f );
		endPos = endPos.WithY( 0f );

		if ( Prediction.FirstTime && MuzzleParticle is not null && muzzle is not null )
		{
			var muzzleFlash = Particles.Create( MuzzleParticle.ResourcePath, muzzle.Value.Position );
			muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
		}

		if ( Game.IsServer )
		{
			FireHelper.EmitFiresAt( startPos, endPos - startPos, 1 );
		}

		if ( FlameDelay > 0 )
		{
			FireEffects();
		}

		FireCount++;
		TimeSinceLastFlame = 0;

		if ( FireCount >= FlamesCount )
		{
			FireCount = 0;

			if ( FlameDelay == 0 )
				FireEffects();

			FireFinished();
		}
	}

	public override void FireFinished()
	{
		base.FireFinished();

		Weapon.PlayScreenSound( To.Everyone, FinishSound );
	}

	private void FireEffects()
	{
		Grub.SetAnimParameter( "fire", true );
		Weapon.PlayScreenSound( To.Everyone, FlameSound );
	}
}
