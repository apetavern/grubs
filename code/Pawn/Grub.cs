using Grubs.Common;
using Grubs.Pawn.Controller;
using Grubs.Terrain;

namespace Grubs.Pawn;

[Title( "Grubs - Container" ), Category( "Grubs" )]
public sealed class Grub : Component, IResolvable
{
	[Sync] public Guid PlayerId { get; set; }
	public Player Player
	{
		get
		{
			var player = Scene.Directory.FindComponentByGuid( PlayerId );
			if ( !player.IsValid() )
				return null;
			return player as Player;
		}
	}

	[Property] public required Health Health { get; set; }
	[Property] public required GrubPlayerController PlayerController { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property, ReadOnly] public Equipment.Equipment ActiveEquipment => Player?.Inventory.ActiveEquipment;

	/// <summary>
	/// Returns true if it is the owning player's turn and this is the player's active Grub.
	/// </summary>
	public bool IsActive => Player is not null && Player.IsActive && Player.ActiveGrub == this;

	public bool Resolved => PlayerController.Velocity.IsNearlyZero( 0.1f ) || IsDead;

	public Transform EyePosition => Transform.World.WithPosition( Transform.Position + Vector3.Up * 24f );

	public Mountable ActiveMountable { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";
	[Sync] public bool IsDead { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Name = Random.Shared.FromList( GrubsConfig.PresetGrubNames );

		if ( !IsProxy )
			InitializeLocal();
	}

	private void InitializeLocal()
	{
		var spawn = GrubsTerrain.Instance.FindSpawnLocation();
		Transform.Position = spawn;
	}

	public void OnHardFall()
	{
		Player?.Inventory.Holster( Player.Inventory.ActiveSlot );
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
		var grub = Game.ActiveScene.GetAllComponents<Grub>().FirstOrDefault( g => g.IsActive );
		if ( grub is null )
			return;

		grub.Health.TakeDamage( new GrubsDamageInfo( hp, grub.Id, grub.Name ) );
	}
}
