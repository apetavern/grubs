using Grubs.Common;
using Grubs.Extensions;
using Grubs.Pawn;
using Grubs.Systems.GameMode;
using Grubs.Systems.Pawn.Grubs.Controller;
using Grubs.UI.World;
using System.Threading;
using static Sandbox.ClothingContainer;

namespace Grubs.Systems.Pawn.Grubs;

[Title( "Grub" ), Category( "Grubs/Pawn" )]
public sealed class Grub : Component, IResolvable
{
	private static readonly Logger Log = new("Grub");
	private CancellationToken token;

	[Sync] public Player Owner { get; private set; }

	[Sync] public string Name { get; private set; }

	[Sync] public Mountable ActiveMountable { get; set; }

	[Sync,Property] public bool IsPoisoned { get; set; }

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

	GameObject PoisonEffects;

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

		var ValidClothing = new List<Clothing.ClothingCategory>()
		{ Clothing.ClothingCategory.Hat,
		Clothing.ClothingCategory.Facial,
		Clothing.ClothingCategory.Hair,
		Clothing.ClothingCategory.HatCap,
		Clothing.ClothingCategory.FacialHairBeard,
		Clothing.ClothingCategory.Eyes,
		Clothing.ClothingCategory.Eyewear,
		Clothing.ClothingCategory.GlassesEye,
		Clothing.ClothingCategory.GlassesSun,
		Clothing.ClothingCategory.Headwear };

		clothingContainer.Clothing.RemoveAll( entry => (entry.Clothing != null && !ValidClothing.Contains( entry.Clothing.Category )) );

		_ = GrubsClothingExtensions.ApplyAsync( clothingContainer, Animator.GrubRenderer, token, ValidClothing );

		Network.Refresh();
	}

	public void SetHatVisible( bool visible )
	{
		foreach (var go in GameObject.Children.Where(go => go.Tags.Has( Clothing.ClothingCategory.Hat.ToString() ) ||
		                                                   go.Tags.Has( Clothing.ClothingCategory.Hair.ToString() )))
		{
			if ( go.Components.TryGet<SkinnedModelRenderer>( out var renderer, FindMode.EverythingInSelf ) )
			{
				renderer.Enabled = visible;
			}
		}
	}

	[Rpc.Owner( NetFlags.HostOnly )]
	public void SetOwner( Player player )
	{
		Owner = player;
		Log.Info( $"Set owner of {this} to {player}." );
	}

	[Rpc.Owner]
	public void SetPoisoned( bool value )
	{
		IsPoisoned = value;
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

	public void OnOwnerTurnEnd()
	{
		if ( IsPoisoned )
		{
			var dmg = new GrubsDamageInfo( 10, new Guid(), "poison", WorldPosition );
			Health.TakeDamage( dmg );
		}
	}

	public void OnGrubTurnEnd()
	{

	}

	protected override void OnUpdate()
	{
		if ( IsPoisoned && Health.CurrentHealth > 0)
		{
			Animator.GrubRenderer.Tint = Color.Green;
			if( PoisonEffects.IsValid() )
			{
				PoisonEffects.WorldPosition = WorldPosition + Vector3.Up * 24f;
			}
			else
			{
				PoisonEffects = GameObject.Clone( "particles/poison/poison_bubbles.prefab" );
				PoisonEffects.Enabled = true;
			}
		}
		else
		{
			if ( PoisonEffects.IsValid() )
			{
				PoisonEffects.Enabled = false;
			}
		}
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
