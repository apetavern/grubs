namespace Grubs;

[Prefab]
public partial class GadgetWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public Prefab GadgetPrefab { get; set; }

	[Prefab, Net]
	public float GadgetDelay { get; set; } = 0f;

	[Prefab, Net]
	public int GadgetCount { get; set; } = 1;

	[Prefab, Net]
	public ParticleSystem MuzzleParticle { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string UseSound { get; set; }

	[Prefab, Net]
	public bool UseTargetPreview { get; set; }

	[Prefab, Net]
	public bool LockPreviewBeforeFire { get; set; }

	[Net]
	private TimeSince TimeSinceLastFire { get; set; } = 0f;

	[Net, Predicted]
	private int FireCount { get; set; } = 0;

	[Net]
	public TargetPreview TargetPreview { get; set; }

	[Net]
	private FiringType FireType { get; set; }

	public override void OnDeploy()
	{
		base.OnDeploy();

		if ( UseTargetPreview )
		{
			FireType = Weapon.FiringType;
			Weapon.FiringType = FiringType.Cursor;
			Weapon.ShowReticle = false;

			if ( Game.IsServer )
			{
				TargetPreview = new();
				TargetPreview.Owner = Grub;
			}
		}
	}

	public override void OnHolster()
	{
		base.OnHolster();

		if ( UseTargetPreview )
		{
			if ( Game.IsServer )
				TargetPreview?.Delete();
			else
				Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( UseTargetPreview )
			TargetPreview?.Simulate( client );

		if ( IsFiring && TimeSinceLastFire >= GadgetDelay && FireCount < GadgetCount )
			Fire();
	}

	public override void FireCursor()
	{
		Weapon.PlayScreenSound( "ui_button_click" );

		if ( UseTargetPreview && TargetPreview.IsValid() && !TargetPreview.IsTargetSet )
		{
			TargetPreview.LockCursor();
			if ( LockPreviewBeforeFire )
			{
				IsFiring = false;
				Weapon.FiringType = FireType;
				Weapon.ShowReticle = true;
				return;
			}
		}

		FireCharged();
	}

	public override void FireInstant()
	{
		FireCharged();
	}

	public override void FireCharged()
	{
		Grub.SetAnimParameter( "fire", true );

		if ( MuzzleParticle is not null )
		{
			var muzzle = Weapon.GetAttachment( "muzzle" );
			if ( Prediction.FirstTime && muzzle is not null )
			{
				var muzzleFlash = Particles.Create( MuzzleParticle.ResourcePath, muzzle.Value.Position );
				muzzleFlash.SetOrientation( 0, muzzle.Value.Rotation.Angles() );
			}
		}

		if ( !Game.IsServer )
			return;

		Weapon.PlayScreenSound( UseSound );
		var gadget = PrefabLibrary.Spawn<Gadget>( GadgetPrefab );
		gadget.OnUse( Grub, Weapon, Charge );

		Charge = MinCharge;
		TimeSinceLastFire = 0;
		FireCount++;

		if ( FireCount >= GadgetCount )
		{
			FireCount = 0;
			FireFinished();
		}
	}

	public override void FireFinished()
	{
		if ( UseTargetPreview )
			TargetPreview?.Hide();

		base.FireFinished();
	}
}
