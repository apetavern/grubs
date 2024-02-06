using Grubs.Equipment;

namespace Grubs.Player;

[Title( "Grubs - Player Inventory" ), Category( "Grubs" )]
public sealed class PlayerInventory : Component
{
	[Property] public required List<GameObject> EquipmentPrefabs { get; set; } = new();
	[Property] public required Grub Grub { get; set; }

	public List<EquipmentComponent> Equipment { get; set; } = new();
	public EquipmentComponent? ActiveEquipment => GetActiveEquipment();
	[Property, ReadOnly, Sync] public int ActiveSlot { get; set; }
	[Property, ReadOnly, Sync] public bool EquipmentActive { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
			EquipmentActive = false;

		foreach ( var prefab in EquipmentPrefabs )
		{
			var go = prefab.Clone();
			if ( go is null )
				return;

			if ( !IsProxy )
			{
				Log.Info( $"Networking Spawning {go.Name}!" );

				go.NetworkSpawn();
			}

			var equipment = go.Components.Get<EquipmentComponent>();
			if ( equipment is null )
				return;

			Equipment.Add( equipment );

			ToggleEquipment( true, Equipment.Count - 1 );
			ToggleEquipment( false, Equipment.Count - 1 );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		if ( Input.Pressed( "toggle_equipment" ) )
		{
			ToggleEquipment( !EquipmentActive, ActiveSlot );

			EquipmentActive = !EquipmentActive;
		}

		if ( Input.Pressed( "next_equipment" ) && EquipmentActive )
		{
			ToggleEquipment( false, ActiveSlot );
			CycleSlot();
			ToggleEquipment( true, ActiveSlot );
		}
	}

	[Broadcast]
	private void ToggleEquipment( bool active, int slot )
	{
		var equipment = Equipment.ElementAt( slot );

		if ( active )
			equipment.Deploy( Grub );
		else
			equipment.Holster();
	}

	private EquipmentComponent? GetActiveEquipment()
	{
		if ( !EquipmentActive )
			return null;

		return Equipment.ElementAt( ActiveSlot );
	}

	private void CycleSlot()
	{
		if ( ActiveSlot >= Equipment.Count - 1 )
		{
			ActiveSlot = 0;
		}
		else
		{
			ActiveSlot++;
		}
	}
}
