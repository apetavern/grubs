using Grubs.Common;
using Grubs.Pawn.Controller;

namespace Grubs.Pawn;

[Title( "Grubs - Container" ), Category( "Grubs" )]
public sealed class Grub : Component, IResolvable
{
	[Sync] public Player Player { get; set; }

	[Property] public required Health Health { get; set; }
	[Property] public required GrubPlayerController PlayerController { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property, ReadOnly] public Equipment.Equipment ActiveEquipment => Player?.Inventory.ActiveEquipment;

	/// <summary>
	/// Returns true if it is the owning player's turn and this is the player's active Grub.
	/// </summary>
	public bool IsActive => Player.IsValid() && Player.IsActive && Player.ActiveGrub == this;

	public bool Resolved => PlayerController.Velocity.IsNearlyZero( 0.1f ) || IsDead;

	public Transform EyePosition => Transform?.World.WithPosition( WorldPosition + Vector3.Up * 24f ) ?? global::Transform.Zero;

	public Mountable ActiveMountable { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";
	[Sync] public bool IsDead { get; set; }

	protected override void OnStart()
	{
		base.OnStart();

		Name = Random.Shared.FromList( GrubsConfig.PresetGrubNames );
	}

	public void OnHardFall()
	{
		if ( !Player.IsValid() || !Player.Inventory.IsValid() )
			return;
		Player?.Inventory.Holster( Player.Inventory.ActiveSlot );
	}

	[ConCmd( "gr_take_dmg" )]
	public static void TakeDmgCmd( float hp )
	{
		var grub = Game.ActiveScene.GetAllComponents<Grub>().FirstOrDefault( g => g.IsActive );
		if ( !grub.IsValid() )
			return;

		grub.Health.TakeDamage( new GrubsDamageInfo( hp, grub.Id, grub.Name ) );
	}
}
