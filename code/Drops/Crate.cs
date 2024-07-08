using Grubs.Equipment;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Drops;

[Title( "Grubs - Crate" ), Category( "Grubs" )]
public sealed class Crate : Component, Component.ITriggerListener
{
	[Property] public DropType DropType { get; set; } = DropType.Weapon;
	[Property] public SoundEvent PickupSound { get; set; }

	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Tags.Has( "player" ) )
		{
			switch ( DropType )
			{
				case DropType.Weapon:
					HandleEquipmentCrateTrigger( other );
					break;
				case DropType.Health:
					HandleHealthCrateTrigger( other );
					break;
				case DropType.Tool:
					HandleEquipmentCrateTrigger( other, true );
					break;
			}
		}
	}

	private void HandleEquipmentCrateTrigger( Collider other, bool isTool = false )
	{
		if ( Connection.Local != other.GameObject.Root.Network.OwnerConnection )
			return;

		PickupEffects();

		string resPath = isTool switch
		{
			true => CrateDrops.GetRandomToolFromCrate(),
			false => CrateDrops.GetRandomWeaponFromCrate()
		};
		var equipmentResource = ResourceLibrary.Get<EquipmentResource>( resPath );

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndAncestors | FindMode.EverythingInChildren );
		var equipment = grub.Player.Inventory.Equipment
			.FirstOrDefault( e => e.Data.Name == equipmentResource.Name );
		equipment?.IncrementAmmo();

		using ( Rpc.FilterInclude( grub.Network.OwnerConnection ) )
		{
			WorldPopupHelper.Instance.CreatePickupPopup( grub.GameObject.Id, equipmentResource.Icon );
		}

		DestroyCrate();
	}

	private void HandleHealthCrateTrigger( Collider other )
	{
		if ( Connection.Local != other.GameObject.Root.Network.OwnerConnection )
			return;

		PickupEffects();

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndAncestors | FindMode.EverythingInChildren );
		if ( grub is null )
			return;

		grub.Health.Heal( 25f );

		DestroyCrate();
	}

	[Authority]
	public void DestroyCrate() => GameObject.Destroy();

	[Broadcast]
	private void PickupEffects()
	{
		Sound.Play( PickupSound, Transform.Position );
	}
}
