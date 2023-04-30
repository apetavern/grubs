namespace Grubs;

[Category( "Grub" )]
public partial class Grub : AnimatedEntity, IResolvable
{
	[BindComponent]
	public GrubController Controller { get; }

	[BindComponent]
	public GrubAnimator Animator { get; }

	public Player Player => Owner as Player;

	public Weapon ActiveWeapon => Player?.Inventory?.ActiveWeapon;

	/// <summary>
	/// Whether it is this Grub's turn.
	/// </summary>
	public bool IsTurn
	{
		get
		{
			if ( Player is null )
				return false;

			return Player.ActiveGrub == this && Player.IsTurn;
		}
	}

	public bool Resolved => Controller.Velocity.IsNearlyZero( 0.1f ) || LifeState is LifeState.Dead or LifeState.Dying || HasBeenDamaged;

	private static readonly Model CitizenGrubModel = Model.Load( "models/citizenworm.vmdl" );

	public Grub()
	{
		Transmit = TransmitType.Always;

		Tags.Add( "player" );
	}

	public Grub( Player player ) : this()
	{
		PostSpawnSetup( player );
	}

	public override void Spawn()
	{
		Model = CitizenGrubModel;

		Health = 100;

		EnableDrawing = true;
		EnableHitboxes = true;

		Components.Create<GrubController>();
		Components.Create<GrubAnimator>();

		Components.Create<UnstuckMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<SquirmMechanic>();
		Components.Create<JumpMechanic>();
		Components.Create<BackflipMechanic>();

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Controller.Hull.Mins, Controller.Hull.Maxs );
	}

	public override void ClientSpawn()
	{
		_ = new UI.GrubWorldPanel( this );
		_ = new UI.TurnBobber( this );
	}

	/// <summary>
	/// PostSpawnSetup is used to handle things we want to handle in Spawn, but 
	/// cannot because the Player hasn't been transmitted to the Grub yet.
	/// </summary>
	private void PostSpawnSetup( Player player )
	{
		DressFromPlayer( player );
	}

	public override void Simulate( IClient client )
	{
		if ( LifeState == LifeState.Dead )
			return;

		Controller?.Simulate( client );
		Animator?.Simulate( client );
	}

	public override void FrameSimulate( IClient client )
	{
		Controller?.FrameSimulate( client );
	}

	private void DressFromPlayer( Player player )
	{
		var clothes = new ClothingContainer();
		clothes.Deserialize( player.AvatarClothingData );

		if ( player.HasCosmeticSelected )
			clothes.Toggle( Player.CosmeticPresets[player.SelectedCosmeticIndex] );

		foreach ( var item in clothes.Clothing )
		{
			var ent = new AnimatedEntity( item.Model, this );

			// Add a tag to the hat so we can reference it later.
			if ( item.Category is Clothing.ClothingCategory.Hat or Clothing.ClothingCategory.Hair )
				ent.Tags.Add( "head" );

			ent.Tags.Add( "clothing" );

			if ( !string.IsNullOrEmpty( item.MaterialGroup ) )
				ent.SetMaterialGroup( item.MaterialGroup );

			if ( item.Category != Clothing.ClothingCategory.Skin )
				continue;

			var skinMaterial = Material.Load( item.SkinMaterial );
			SetMaterialOverride( skinMaterial, "skin" );
		}
	}

	public void SetHatVisible( bool visible )
	{
		var hats = Children.OfType<AnimatedEntity>().Where( child => child.Tags.Has( "head" ) );

		foreach ( var hat in hats )
			hat.EnableDrawing = visible;
	}
}
