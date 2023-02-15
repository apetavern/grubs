namespace Grubs;

[Category( "Grub" )]
public partial class Grub : AnimatedEntity, INameTag
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

	public Color Color => Player.Color;

	private static readonly Model CitizenGrubModel = Model.Load( "models/citizenworm.vmdl" );

	public Grub()
	{
		Transmit = TransmitType.Always;

		Tags.Add( "player" );
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

	public override void Simulate( IClient client )
	{
		Controller?.Simulate( client );
		Animator?.Simulate( client );
	}

	public override void FrameSimulate( IClient client )
	{
		Controller?.FrameSimulate( client );
	}
}
