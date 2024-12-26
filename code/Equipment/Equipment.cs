using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Pawn;
using Grub = Grubs.Systems.Pawn.Grubs.Grub;

namespace Grubs.Equipment;

[Title( "Grubs - Equipment" ), Category( "Equipment" )]
public class Equipment : Component
{
	[Property] public required string Name { get; set; } = "";
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;
	[Property, ResourceType( "jpg" )] public string Icon { get; set; } = "";
	[Property] public bool CameraCanZoom { get; set; } = true;
	[Property] public bool ShouldShowAimReticle { get; set; } = false;
	[Property] public int UnlockDelay { get; set; } = 0;

	/// <summary>
	/// Data from the GameResource for this Equipment.
	/// </summary>
	[Property, ResourceType( "geq" )] public required EquipmentResource Data { get; set; }

	[Property, Sync, ReadOnly] public int SlotIndex { get; set; }

	[Sync] public int Ammo { get; set; }

	public bool Deployed { get; set; }

	public Grub Grub { get; set; }

	// public int RoundsUntilUnlock => (UnlockDelay - Gamemodes.Gamemode.GetCurrent().RoundsPassed).Clamp( 0, int.MaxValue );
	// public bool Unlocked => RoundsUntilUnlock == 0 || Ammo == -1;
	public bool IsAvailable => Ammo != 0;

	protected override void OnStart()
	{
		base.OnStart();

		// If we've set ammo before OnStart (e.g. Infinite Ammo), don't overwrite it.
		if ( Ammo != -1 )
			Ammo = Data.DefaultAmmo;

		Holster();
	}

	protected override void OnUpdate()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if ( !Grub.IsValid() || !Grub.PlayerController.IsValid() )
			return;

		var show = Grub.PlayerController.ShouldShowWeapon() && Deployed;
		Model.Enabled = show;
	}

	public void Deploy( Grub grub )
	{
		Grub = grub;

		if ( !IsAvailable )
			return;

		if ( Components.TryGet( out Weapon weapon ) )
			weapon?.OnDeploy();

		if ( !GameObject.IsValid() || !grub.IsValid() || !Model.IsValid() )
			return;

		GameObject.SetParent( grub.GameObject, false );
		Model.BoneMergeTarget = grub.Components.Get<SkinnedModelRenderer>();
		Model.Enabled = true;
		Deployed = true;

		if ( IsProxy )
			return;

		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.AllowZooming = CameraCanZoom;
	}

	public void Holster()
	{
		if ( Grub.IsValid() && Grub.Owner.IsValid() )
			GameObject.SetParent( Grub.Owner.GameObject );

		if ( Components.TryGet( out Weapon weapon ) )
		{
			weapon.OnHolster();

			if ( weapon.TimesUsed > 0 )
				UseAmmo();

			weapon.TimesUsed = 0;
		}

		Grub = null;

		Model.BoneMergeTarget = null;
		Model.Enabled = false;
		Deployed = false;
		
		if ( GrubFollowCamera.Local.IsValid() )
			GrubFollowCamera.Local.AllowZooming = true;
	}

	public void UseAmmo()
	{
		if ( Ammo == -1 )
			return;
		Ammo -= 1;
	}

	public void IncrementAmmo()
	{
		if ( Ammo == -1 )
			return;
		Ammo += 1;
	}
}
