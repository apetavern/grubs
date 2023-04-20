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

	public Color Color => Player.Preferences.Color;

	public bool Resolved => Controller.Velocity.IsNearlyZero( 0.1f ) || LifeState is LifeState.Dead or LifeState.Dying || HasBeenDamaged;

	private static readonly Model CitizenGrubModel = Model.Load( "models/citizenworm.vmdl" );

	public Grub()
	{
		Transmit = TransmitType.Always;

		Tags.Add( "player" );
	}

	public Grub( IClient client ) : this()
	{
		PostSpawnSetup( client );
	}

	public override void Spawn()
	{
		Model = CitizenGrubModel;

		Name = Game.Random.FromArray( GrubsConfig.GrubNames );
		Health = 100;

		EnableDrawing = true;
		EnableHitboxes = true;

		Components.Create<GrubController>();
		Components.Create<GrubAnimator>();

		Components.Create<UnstuckMechanic>();
		Components.Create<AirMoveMechanic>();
		Components.Create<SquirmMechanic>();
		Components.Create<JumpMechanic>();

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Controller.Hull.Mins, Controller.Hull.Maxs );
	}

	public override void ClientSpawn()
	{
		_ = new GrubWorldPanel( this );
		_ = new TurnBobber( this );
	}

	/// <summary>
	/// PostSpawnSetup is used to handle things we want to handle in Spawn, but 
	/// cannot because the Client hasn't been transmitted to the Grub yet.
	/// </summary>
	private void PostSpawnSetup( IClient client )
	{
		DressFromClient( client );
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

	private void DressFromClient( IClient client )
	{
		var clothes = new ClothingContainer();
		clothes.LoadFromClient( client );

		// Skin tone
		var skinTone = clothes.Clothing.FirstOrDefault( model => model.Model == "models/citizenworm.vmdl" );
		SetMaterialGroup( skinTone?.MaterialGroup );

		// We only want the hair/hats so we won't use the logic built into Clothing
		var items = clothes.Clothing.Where( item =>
			item.Category is Clothing.ClothingCategory.Hair or Clothing.ClothingCategory.Hat
				or Clothing.ClothingCategory.Facial or Clothing.ClothingCategory.Skin
		);

		foreach ( var item in items )
		{
			var ent = new AnimatedEntity( item.Model, this );

			// Add a tag to the hat so we can reference it later.
			if ( item.Category is Clothing.ClothingCategory.Hat or Clothing.ClothingCategory.Hair )
				ent.Tags.Add( "head" );

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
