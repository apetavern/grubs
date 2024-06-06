using Grubs.Gamemodes;
using Grubs.UI.Inventory;

namespace Grubs.Pawn;

[Title( "Grubs - Player Inventory" ), Category( "Grubs" )]
public sealed class PlayerInventory : Component
{
	[Property] public required List<GameObject> EquipmentPrefabs { get; set; } = new();
	[Property] public required Player Player { get; set; }

	public List<Equipment.Equipment> Equipment { get; set; } = new();
	public Equipment.Equipment ActiveEquipment => GetActiveEquipment();
	[Property, ReadOnly, Sync] public int ActiveSlot { get; set; }
	[Property, ReadOnly, Sync] public bool EquipmentActive { get; set; }

	public bool InventoryOpen { get; set; }
	public static PlayerInventory Local { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
			Local = this;
	}

	public void Cleanup()
	{
		for ( int i = 0; i < Equipment.Count; i++ )
		{
			Log.Info( Equipment[i].Name );
			Equipment[i].GameObject.Destroy();

		}
		Equipment.Clear();
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

			var equipment = go.Components.Get<Equipment.Equipment>();

			Equipment.Add( equipment );

			var slotIndex = Equipment.Count - 1;

			equipment.SlotIndex = slotIndex;
			equipment.Deploy( Player.ActiveGrub );
			equipment.Holster();
		}

		//Sort by drop chance (0 drop chance gets put to the front)
		Equipment = Equipment.OrderBy( X => X.Data.DropChance != 0 ? -X.Data.DropChance : -100f ).ToList();
		//Put the tools at the front of the line
		Equipment = Equipment.OrderBy( X => X.Data.Type != Grubs.Equipment.EquipmentType.Tool ).ToList();

		//Re-set equipment slot indexes
		foreach ( var item in Equipment )
		{
			item.SlotIndex = Equipment.IndexOf( item );
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( IsProxy )
			return;

		if ( Input.Pressed( "toggle_inventory" ) )
		{
			InventoryOpen = !InventoryOpen;
		}

		if ( Input.Pressed( "next_equipment" ) )
		{
			CycleSlot();
		}

		if ( Input.Pressed( "previous_equipment" ) )
		{
			CycleSlot( false );
		}
	}

	public void EquipItem( Equipment.Equipment equipment )
	{
		if ( Player.HasFiredThisTurn )
			return;

		Holster( ActiveSlot );
		var index = Equipment.IndexOf( equipment );
		if ( index == -1 )
			return;
		ActiveSlot = index;
		Equip( ActiveSlot );

		if ( !Input.UsingController )
			InventoryOpen = false;
	}

	[Broadcast]
	public void Equip( int slot )
	{
		if ( Gamemode.FFA.TurnIsChanging || !Player.IsActive )
			return;

		var equipment = GetHolsteredEquipment( slot );
		if ( equipment is null || !equipment.IsValid() )
		{
			Log.Warning( $"Can't equip: nothing at slot {slot} or is null" );
			return;
		}

		EquipmentActive = true;
		equipment.Deploy( Player.ActiveGrub );
	}

	[Broadcast]
	public void Holster( int slot )
	{
		var equipment = GetActiveEquipment( slot );

		if ( equipment is null || !equipment.IsValid() )
			return;

		EquipmentActive = false;
		equipment.Holster();
	}

	private Equipment.Equipment GetActiveEquipment()
	{
		return !EquipmentActive ? null : GetActiveEquipment( ActiveSlot );
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

	private Equipment.Equipment GetHolsteredEquipment( int slot )
	{
		return Player.GameObject.Components
			.GetAll<Equipment.Equipment>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
	}

	private Equipment.Equipment GetActiveEquipment( int slot )
	{
		return Player.ActiveGrub.GameObject.Components
			.GetAll<Equipment.Equipment>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
	}

	private void CycleSlot( bool forwards = true )
	{
		Holster( ActiveSlot );
		var slot = forwards ? GetNextSlot() : GetPrevSlot();
		ActiveSlot = slot;
		Equip( ActiveSlot );
	}
}
