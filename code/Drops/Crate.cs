using Grubs.Equipment;
using Grubs.Helpers;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;

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
		if ( Connection.Local != other.GameObject.Root.Network.Owner )
			return;

		PickupEffects();

		string resPath = isTool switch
		{
			true => CrateDrops.GetRandomToolFromCrate(),
			false => CrateDrops.GetRandomWeaponFromCrate()
		};
		var equipmentResource = ResourceLibrary.Get<EquipmentResource>( resPath );

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndAncestors | FindMode.EverythingInChildren );
		var equipment = grub.Owner.Inventory.Equipment
			.FirstOrDefault( e => e.Data.Name == equipmentResource.Name );
		equipment?.IncrementAmmo();

		using ( Rpc.FilterInclude( grub.Network.Owner ) )
		{
			WorldPopupHelper.Instance.CreatePickupPopup( grub.GameObject.Id, equipmentResource.Icon );
		}

		DestroyCrate();
	}

	private void HandleHealthCrateTrigger( Collider other )
	{
		if ( Connection.Local != other.GameObject.Root.Network.Owner )
			return;

		PickupEffects();

		var grub = other.GameObject.Root.Components.Get<Grub>( FindMode.EverythingInSelfAndAncestors | FindMode.EverythingInChildren );
		if ( !grub.IsValid() || !grub.Health.IsValid() )
			return;

		grub.Health.Heal( 25f );

		DestroyCrate();
	}

	[Rpc.Owner]
	public void DestroyCrate() => GameObject.Destroy();

	[Rpc.Broadcast]
	private void PickupEffects()
	{
		Sound.Play( PickupSound, WorldPosition );
	}
}
