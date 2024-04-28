using Grubs.Equipment;
using Grubs.Gamemodes;

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

	public bool InventoryOpen { get; set; }
	public static PlayerInventory Local { get; set; }

	protected override void OnStart()
	{
		if ( !IsProxy )
			Local = this;
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
			equipment.Deploy( Player.ActiveGrub );
			equipment.Holster();
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
	}

	public void EquipItem( EquipmentComponent equipment )
	{
		Holster( ActiveSlot );
		var index = Equipment.IndexOf( equipment );
		if ( index == -1 )
			return;
		ActiveSlot = index;
		Equip( ActiveSlot );
	}

	[Broadcast]
	public void Equip( int slot )
	{
		if ( Gamemode.FFA.TurnIsChanging )
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

	private EquipmentComponent GetActiveEquipment()
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

	private EquipmentComponent GetHolsteredEquipment( int slot )
	{
		return Player.GameObject.Components
			.GetAll<EquipmentComponent>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
	}

	private EquipmentComponent GetActiveEquipment( int slot )
	{
		return Player.ActiveGrub.GameObject.Components
			.GetAll<EquipmentComponent>( FindMode.EverythingInSelfAndDescendants )
			.FirstOrDefault( x => x.SlotIndex == slot );
	}

	private void CycleSlot( bool forwards = true )
	{
		var slot = forwards ? GetNextSlot() : GetPrevSlot();
		ActiveSlot = slot;
	}
}
