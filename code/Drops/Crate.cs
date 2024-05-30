using Grubs.Equipment;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Drops;

[Title( "Grubs - Crate" ), Category( "Grubs" )]
public sealed class Crate : Component, Component.ITriggerListener
{
	[Property] public DropType DropType { get; set; } = DropType.Weapon;

	private bool _pickedUp = false;

	public void OnTriggerEnter( Collider other )
	{
		// kidd: s&box seems to be firing OnTriggerEnter for every collider on the GameObject,
		// instead of specifically trigger colliders, causing players to pick up two items.
		// Stupid workaround until this is fixed.
		if ( _pickedUp )
			return;

		if ( other.GameObject.Tags.Has( "player" ) )
		{
			_pickedUp = true;
			switch ( DropType )
			{
				case DropType.Weapon:
					HandleWeaponCrateTrigger( other );
					break;
				case DropType.Health:
					HandleHealthCrateTrigger( other );
					break;
			}
		}
	}

	private void HandleWeaponCrateTrigger( Collider other )
	{
		var resPath = CrateDrops.GetRandomWeaponFromCrate();
		var equipmentResource = ResourceLibrary.Get<EquipmentResource>( resPath );

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndChildren );
		var equipment = grub.Player.Inventory.Equipment
			.FirstOrDefault( e => e.Data.Name == equipmentResource.Name );
		equipment?.IncrementAmmo();

		WorldPopupHelper.Local.CreatePickupPopup( grub.GameObject.Id, equipmentResource.Icon );
		GameObject.Destroy();
	}

	private void HandleHealthCrateTrigger( Collider other )
	{
		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndChildren );
		if ( grub is null )
			return;

		grub.Health.Heal( 25f );
		GameObject.Destroy();
	}
}
