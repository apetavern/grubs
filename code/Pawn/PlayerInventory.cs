using Grubs.Equipment;

namespace Grubs.Pawn;

[Title( "Grubs - Player Inventory" ), Category( "Grubs" )]
public sealed class PlayerInventory : Component
{
	[Property] public required List<GameObject> EquipmentPrefabs { get; set; } = new();
	[Property] public required Player Player { get; set; }

	public List<EquipmentComponent> Equipment { get; set; } = new();
	public EquipmentComponent ActiveEquipment => GetActiveEquipment();
	[Property, ReadOnly, Sync] public int ActiveSlot { get; set; }
	[Property, ReadOnly, Sync] public bool EquipmentActive { get; set; }

	protected override void OnStart()
	{
	}

	[Broadcast( NetPermission.HostOnly )]
	public void InitializeWeapons()
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
			equipment.Deploy( Player.ActiveGrub! );
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
			CycleItems( true );
		}

		if ( Input.Pressed( "prev_equipment" ) && EquipmentActive )
		{
			CycleItems( false );
		}
	}

	private void CycleItems( bool forwards )
	{
		ToggleEquipment( false, ActiveSlot );
		CycleSlot( forwards );
		ToggleEquipment( true, ActiveSlot );
	}

	[Broadcast]
	public void ToggleEquipment( bool active, int slot )
	{
		EquipmentActive = active;
		var equipment = GetEquipmentAtSlot( slot );

		if ( equipment is null )
			return;

		if ( active )
			equipment.Deploy( Player.ActiveGrub! );
		else
			equipment.Holster();
	}

	private EquipmentComponent GetActiveEquipment()
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

	public int GetPrevSlot()
	{
		if ( ActiveSlot == 0 )
			return Equipment.Count - 1;
		return ActiveSlot - 1;
	}

	private EquipmentComponent GetEquipmentAtSlot( int slot )
	{
		return Player.ActiveGrub?.GameObject.Components
			.GetAll<EquipmentComponent>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
	}

	private void CycleSlot( bool forwards = true )
	{
		var slot = forwards ? GetNextSlot() : GetPrevSlot();
		ActiveSlot = slot;
	}
}
