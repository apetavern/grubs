using Grubs.Common;
using Grubs.Equipment;
using Grubs.Player.Controller;

namespace Grubs.Player;

[Title( "Grubs - Container" ), Category( "Grubs" )]
public sealed class Grub : Component
{
	[Property] public required HealthComponent Health { get; set; }
	[Property] public required GrubPlayerController PlayerController { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property] public required PlayerInventory Inventory { get; set; }
	[Property, ReadOnly] public EquipmentComponent? ActiveEquipment => Inventory.ActiveEquipment;

	[Sync] public string Name { get; set; } = "Grubby";

	protected override void OnStart()
	{
		base.OnStart();

		if ( !IsProxy )
			InitializeLocal();
	}

	private void InitializeLocal()
	{
		if ( GrubFollowCamera.Local is not null )
			GrubFollowCamera.Local.Target = GameObject;
	}

	public void OnHardFall()
	{
		Inventory.ToggleEquipment( false, Inventory.ActiveSlot );
	}

	// public void Respawn()
	// {
	// 	var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
	// 	var spawn = Random.Shared.FromArray( spawnPoints )?.Transform.World ?? Transform.World;
	// 	Health.Heal( 150f );
	// 	Transform.Position = spawn.Position;
	// }

	[ConCmd( "gr_take_dmg" )]
	public static void TakeDmgCmd( float hp )
	{
		var grub = GameManager.ActiveScene.GetAllComponents<Grub>().FirstOrDefault();
		if ( grub is null )
			return;

		grub.Health.TakeDamage( hp );
	}
}
