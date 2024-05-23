﻿using Grubs.Equipment.Weapons;
using Grubs.Pawn;

namespace Grubs.Equipment;

[Title( "Grubs - Equipment" ), Category( "Equipment" )]
public class Equipment : Component
{
	[Property] public required string Name { get; set; } = "";
	[Property] public required SkinnedModelRenderer Model { get; set; }
	[Property] public HoldPose HoldPose { get; set; } = HoldPose.None;
	[Property, ResourceType( "jpg" )] public string Icon { get; set; } = "";

	/// <summary>
	/// Data from the GameResource for this Equipment.
	/// </summary>
	[Property, ResourceType( "geq" )] public required EquipmentResource Data { get; set; }

	[Property, Sync, ReadOnly] public int SlotIndex { get; set; }

	[Sync] public int Ammo { get; set; }

	public bool Deployed { get; set; }

	public Grub Grub { get; set; }

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
		
		if ( Ammo == 0 )
			return;

		var target = grub.GameObject.GetAllObjects( true ).First( c => c.Name == "hold_L" );
		GameObject.SetParent( grub.GameObject, false );
		Model.BoneMergeTarget = grub.Components.Get<SkinnedModelRenderer>();
		Model.Enabled = true;
		Deployed = true;
	}

	public void Holster()
	{
		if ( Grub is not null )
			GameObject.SetParent( Grub.Player.GameObject );

		if ( Components.TryGet( out Weapon weapon ) )
			weapon.TimesUsed = 0;

		Grub = null;

		Model.BoneMergeTarget = null;
		Model.Enabled = false;
		Deployed = false;
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
