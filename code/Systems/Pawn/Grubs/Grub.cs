using Grubs.Common;
using Grubs.Pawn;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn.Grubs.Controller;
using Grubs.UI.World;

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
	[Property] public GrubPlayerController PlayerController { get; set; }
	[Property] public GrubCharacterController CharacterController { get; set; }
	[Property] public GrubAnimator Animator { get; set; }
	[Property] private TurnIndicator TurnIndicator { get; set; }
	
	public Equipment.Equipment ActiveEquipment => Owner?.Inventory.ActiveEquipment;
	
	public float HealthPercentage => Health.CurrentHealth / Health.MaxHealth;

	public Transform EyePosition => WorldTransform.WithPosition( WorldPosition + Vector3.Up * 24f );

	private static readonly List<Clothing.ClothingCategory> ValidClothingCategories = new()
	{
		Clothing.ClothingCategory.Hat,
		Clothing.ClothingCategory.Facial,
		Clothing.ClothingCategory.Hair
	};

	protected override void OnStart()
	{
		if ( !IsProxy )
			Name = Game.Random.FromList( GrubsConfig.PresetGrubNames );

		if ( Networking.IsHost )
		{
			var avatarData = Network.Owner.GetUserData( "avatar" );
			DressGrub( avatarData );
		}
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	private void DressGrub( string avatarData )
	{
		var clothingContainer = ClothingContainer.CreateFromJson( avatarData );
		
		foreach ( var clothingEntry in clothingContainer.Clothing )
		{
			if ( !ValidClothingCategories.Contains( clothingEntry.Clothing.Category ) )
				continue;

			var go = new GameObject();
			go.SetParent( GameObject );
			go.Tags.Add( "clothing" );
			go.Name = $"Clothing - {clothingEntry.Clothing.ResourceName}";
			var clothingModel = go.Components.Create<SkinnedModelRenderer>();
			clothingModel.Model = Model.Load( clothingEntry.Clothing.Model );
			clothingModel.BoneMergeTarget = Animator.GrubRenderer;
		}
		
		Network.Refresh();
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

	public void OnTurnStart()
	{
		TurnIndicator.Show( this );
	}

	public override string ToString()
	{
		return $"Grub {Name} on {GameObject?.Name ?? "null"}";
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
