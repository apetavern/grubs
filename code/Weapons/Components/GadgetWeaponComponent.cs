namespace Grubs;

[Prefab]
public partial class GadgetWeaponComponent : WeaponComponent
{
	[Prefab, Net]
	public Prefab GadgetPrefab { get; set; }

	[Prefab, Net]
	public float DelayPerUse { get; set; } = 0f;

	[Prefab, Net]
	public int TotalUseCount { get; set; } = 1;

	[Prefab, Net]
	public int GadgetsPerUse { get; set; } = 1;

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
	private int UseCount { get; set; } = 0;

	[Net]
	public TargetPreview TargetPreview { get; set; }

	[Net]
	private FiringType _fireType { get; set; }

	public override void OnDeploy()
	{
		base.OnDeploy();

		if ( UseTargetPreview )
		{
			_fireType = Weapon.FiringType;
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
			Weapon.FiringType = _fireType;
			TargetPreview.Hide();

			if ( Game.IsClient )
				Grub.Player.GrubsCamera.AutomaticRefocus = true;
		}
	}

	public override void Simulate( IClient client )
	{
		base.Simulate( client );

		if ( UseTargetPreview )
			TargetPreview?.Simulate( client );

		if ( IsFiring && TimeSinceLastFire >= DelayPerUse && UseCount < TotalUseCount )
			Fire();
	}

	public override void FireCursor()
	{
		Weapon.PlaySound( "ui_button_click" );

		if ( UseTargetPreview && TargetPreview.IsValid() && !TargetPreview.IsTargetSet )
		{
			TargetPreview.LockCursor();
			if ( LockPreviewBeforeFire )
			{
				IsFiring = false;
				Weapon.FiringType = _fireType;
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

		Weapon.PlaySound( UseSound );

		for ( int i = 0; i < GadgetsPerUse; i++ )
		{
			var gadget = PrefabLibrary.Spawn<Gadget>( GadgetPrefab );
			gadget.OnUse( Grub, Weapon, Charge );
		}

		Charge = MinCharge;
		TimeSinceLastFire = 0;
		UseCount++;

		if ( UseCount >= TotalUseCount )
		{
			UseCount = 0;
			FireFinished();
		}
	}

	public override void FireFinished()
	{
		if ( UseTargetPreview )
		{
			Weapon.FiringType = FiringType.Cursor;
			TargetPreview.UnlockCursor();
		}

		base.FireFinished();
	}
}
