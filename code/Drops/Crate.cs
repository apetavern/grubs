using Grubs.Equipment;
using Grubs.Helpers;
using Grubs.Pawn;

namespace Grubs.Drops;

[Title( "Grubs - Crate" ), Category( "Grubs" )]
public sealed class Crate : Component, Component.ITriggerListener
{
	[Property] public DropType DropType { get; set; } = DropType.Weapon;

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
		string resPath = isTool switch
		{
			true => CrateDrops.GetRandomToolFromCrate(),
			false => CrateDrops.GetRandomWeaponFromCrate()
		};
		var equipmentResource = ResourceLibrary.Get<EquipmentResource>( resPath );

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndChildren );
		var equipment = grub.Player.Inventory.Equipment
			.FirstOrDefault( e => e.Data.Name == equipmentResource.Name );
		equipment?.IncrementAmmo();

		WorldPopupHelper.Instance.CreatePickupPopup( grub.GameObject.Id, equipmentResource.Icon );
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
