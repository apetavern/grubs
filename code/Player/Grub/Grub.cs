namespace Grubs;

public partial class Grub : AnimatedEntity
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

			return Player.ActiveGrub == this;
		}
	}

	private static readonly Model CitizenGrubModel = Model.Load( "models/citizenworm.vmdl" );

	public Grub()
	{
		Transmit = TransmitType.Always;
	}

	public override void Spawn()
	{
		Model = CitizenGrubModel;
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		EnableDrawing = true;
		EnableHitboxes = true;

		Components.Create<GrubController>();
		Components.Create<GrubAnimator>();

		Components.Create<AirMoveMechanic>();
		Components.Create<SquirmMechanic>();
		Components.Create<JumpMechanic>();

		Tags.Add( "player" );
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

	public override void OnKilled()
	{
		Controller?.Remove();
		Animator?.Remove();
	}
}
