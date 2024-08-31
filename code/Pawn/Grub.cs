using Grubs.Common;
using Grubs.Pawn.Controller;

namespace Grubs.Pawn;

[Title( "Grubs - Container" ), Category( "Grubs" )]
public sealed class Grub : Component, IResolvable
{
	[Sync] public Player Player { get; set; }

	[Property] public required Health Health { get; set; }
	[Property] public SkinnedModelRenderer GrubRenderer { get; set; }
	[Property] public GrubPlayerController PlayerController { get; set; }
	[Property] public GrubCharacterController CharacterController { get; set; }
	[Property] public GrubAnimator Animator { get; set; }
	[Property, ReadOnly] public Equipment.Equipment ActiveEquipment => Player.IsValid() ? Player?.Inventory.ActiveEquipment : null;

	/// <summary>
	/// Returns true if it is the owning player's turn and this is the player's active Grub.
	/// </summary>
	public bool IsActive => Player.IsValid() && Player.IsActive && Player.ActiveGrub == this;

	public bool Resolved => PlayerController.Velocity.IsNearlyZero( 0.1f ) || IsDead;

	public Transform EyePosition => Transform?.World.WithPosition( Transform.Position + Vector3.Up * 24f ) ?? global::Transform.Zero;

	public Mountable ActiveMountable { get; set; }

	[Sync] public string Name { get; set; } = "Grubby";
	[Sync] public bool IsDead { get; set; }

	private List<ModelRenderer> ClothingRenderers { get; set; } = new();

	private List<Clothing.ClothingCategory> CategoryWhiteList { get; set; } = new()
	{
		Clothing.ClothingCategory.Hat,
		Clothing.ClothingCategory.Facial,
		Clothing.ClothingCategory.Hair,
	};

	protected override void OnStart()
	{
		base.OnStart();

		Name = Random.Shared.FromList( GrubsConfig.PresetGrubNames );

		var clothing = new ClothingContainer();
		clothing.Deserialize( Network.OwnerConnection.GetUserData( "avatar" ) );
		clothing.Clothing?.RemoveAll( c => !CategoryWhiteList.Contains( c.Clothing.Category ) );
		clothing.Apply( GrubRenderer );

		var renderers = Components.GetAll<ModelRenderer>().Where( r => r.Tags.Contains( "clothing" ) ).ToList();
		foreach ( var renderer in renderers )
		{
			if ( !renderer.IsValid() )
				continue;

			if ( renderer.Tags.Contains( "clothing" ) )
				renderer.GameObject.SetParent( GameObject );
		}
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
