using Grubs.Equipment.Weapons;
using Grubs.Pawn;

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

	public bool Unlocked => Gamemodes.Gamemode.Current.RoundsPassed >= UnlockDelay || Ammo == -1;
	public bool IsAvailable => Ammo != 0 && Unlocked;

	protected override void OnStart()
	{
		base.OnStart();

		Ammo = Data.DefaultAmmo;

		Holster();
	}

	protected override void OnUpdate()
	{
		UpdateVisibility();
	}

	private void UpdateVisibility()
	{
		if ( Grub is null )
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
			weapon.OnDeploy();

		var target = grub.GameObject.GetAllObjects( true ).First( c => c.Name == "hold_L" );
		GameObject.SetParent( grub.GameObject, false );
		Model.BoneMergeTarget = grub.Components.Get<SkinnedModelRenderer>();
		Model.Enabled = true;
		Deployed = true;
		GrubFollowCamera.Local.AllowZooming = CameraCanZoom;
	}

	public void Holster()
	{
		if ( Grub is not null && Grub.Player is not null )
			GameObject.SetParent( Grub.Player.GameObject );

		if ( Components.TryGet( out Weapon weapon ) )
		{
			weapon.TimesUsed = 0;
			weapon.OnHolster();
		}

		Grub = null;

		Model.BoneMergeTarget = null;
		Model.Enabled = false;
		Deployed = false;
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
