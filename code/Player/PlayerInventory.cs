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
		if ( IsProxy )
			return;

		EquipmentActive = false;

		foreach ( var prefab in EquipmentPrefabs )
		{
			var go = prefab.Clone();
			go.NetworkSpawn();

			var equipment = go.Components.Get<EquipmentComponent>();

			Equipment.Add( equipment );

			var slotIndex = Equipment.Count - 1;

			equipment.SlotIndex = slotIndex;
			equipment.Deploy( Grub );
			equipment.Holster();
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
		}

		if ( Input.Pressed( "next_equipment" ) && EquipmentActive )
		{
			ToggleEquipment( false, ActiveSlot );
			CycleSlot();
			ToggleEquipment( true, ActiveSlot );
		}
	}

	[Broadcast]
	public void ToggleEquipment( bool active, int slot )
	{
		EquipmentActive = active;
		var equipment = GetEquipmentAtSlot( slot );

		if ( equipment is null )
			return;

		if ( active )
			equipment.Deploy( Grub );
		else
			equipment.Holster();
	}

	private EquipmentComponent? GetActiveEquipment()
	{
		if ( !EquipmentActive )
			return null;

		return GetEquipmentAtSlot( ActiveSlot );
	}

	public int GetNextSlot()
	{
		if ( ActiveSlot >= Equipment.Count - 1 )
			return 0;
		return ActiveSlot + 1;
	}

	private EquipmentComponent? GetEquipmentAtSlot( int slot )
	{
		return GameObject.Components.GetAll<EquipmentComponent>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
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
