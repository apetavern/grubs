using Grubs.Common;
using Grubs.Pawn;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn.Grubs.Controller;

namespace Grubs.Systems.Pawn.Grubs;

[Title("Grub"), Category("Grubs/Pawn")]
public sealed class Grub : Component, IResolvable
{
	private static readonly Logger Log = new( "Grub" );
	
	[Sync]
	public Player Owner { get; private set; }
	
	[Sync]
	public string Name { get; private set; }
	
	[Sync]
	public Mountable ActiveMountable { get; set; }

	public bool IsActive()
	{
		return BaseGameMode.Current.IsValid() && BaseGameMode.Current.IsGrubActive( this );
	} 
	
	[Property] public Health Health { get; set; }
	[Property] public required GrubPlayerController PlayerController { get; set; }
	[Property] public required GrubCharacterController CharacterController { get; set; }
	[Property] public required GrubAnimator Animator { get; set; }
	[Property, ReadOnly] public Equipment.Equipment ActiveEquipment => Owner?.Inventory.ActiveEquipment;
	
	public float HealthPercentage => Health.CurrentHealth / Health.MaxHealth;

	public Transform EyePosition => WorldTransform.WithPosition( WorldPosition + Vector3.Up * 24f );

	protected override void OnStart()
	{
		Name = Game.Random.FromList( GrubsConfig.PresetGrubNames );
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	public void SetOwner( Player player )
	{
		Owner = player;
		Log.Info( $"Set owner of {this} to {player}." );
	}

	public void OnHardFall()
	{
		if ( !Owner.IsValid() || !Owner.Inventory.IsValid() )
			return;
		Owner?.Inventory.HolsterActive();
	}

	public override string ToString()
	{
		return $"{Name} (Owner: {Owner})";
	}

	[ConCmd( "gr_set_active_grub_health" )]
	public static void SetActiveGrubHealth( float health )
	{
		if ( BaseGameMode.Current is FreeForAll freeForAll )
		{
			freeForAll.ActivePlayer.ActiveGrub.Health.CurrentHealth = health;
		}
	}

	public bool Resolved => IsResolved();

	private bool IsResolved()
	{
		if ( !PlayerController.IsValid() )
			return true;
		
		return PlayerController.Velocity.IsNearlyZero( 0.1f ) || HealthPercentage <= 0f;
	}
}
