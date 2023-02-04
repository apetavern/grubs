namespace Grubs;

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
		SetupPhysicsFromModel( PhysicsMotionType.Keyframed );

		Name = Game.Random.FromArray( GrubsConfig.GrubNames );
		Health = 100;

		EnableDrawing = true;
		EnableHitboxes = true;

		Components.Create<GrubController>();
		Components.Create<GrubAnimator>();

		Components.Create<AirMoveMechanic>();
		Components.Create<SquirmMechanic>();
		Components.Create<JumpMechanic>();
	}

	public override void Simulate( IClient client )
	{
		Controller?.Simulate( client );
		Animator?.Simulate( client );

		var game = GrubsGame.Instance;
		var world = game.World;

		if ( Game.IsServer && Input.Down( InputButton.PrimaryAttack ) && IsTurn )
		{
			var aimRay = Trace.Ray( AimRay, 80f ).WithTag( "solid" ).Ignore( this ).Run();
			if ( aimRay.Hit )
			{
				var min = new Vector3( aimRay.EndPosition.x - 16f, -32, aimRay.EndPosition.z - 16f );
				var max = new Vector3( aimRay.EndPosition.x + 16f, 32, aimRay.EndPosition.z + 16f );
				DebugOverlay.Box( min, max );
				world.SubtractDefault( min, max );
			}
		}
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
