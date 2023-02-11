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

	[Net]
	public bool HasBeenDamaged { get; set; }

	public bool ShouldTakeDamage { get; set; }

	public Queue<DamageInfo> DamageQueue { get; set; } = new();

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

		Components.Create<AirMoveMechanic>();
		Components.Create<SquirmMechanic>();
		Components.Create<JumpMechanic>();

		SetupPhysicsFromAABB( PhysicsMotionType.Keyframed, Controller.Hull.Mins, Controller.Hull.Maxs );
	}

	public override void Simulate( IClient client )
	{
		Controller?.Simulate( client );
		Animator?.Simulate( client );

		var game = GrubsGame.Instance;
		var world = game.World;

		if ( Game.IsServer && Input.Down( InputButton.Flashlight ) && IsTurn )
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

	public override void TakeDamage( DamageInfo info )
	{
		if ( !Game.IsServer )
			return;

		// Quick and temporary method to get rid of a Grub upon falling out of bounds.
		if ( info.HasTag( "outofarea" ) )
		{
			Health = 0;
			Player.Inventory.UnsetActiveWeapon();
			EnableDrawing = false;
			Components.Remove( Controller );
			LifeState = LifeState.Dead;
			return;
		}

		if ( !ShouldTakeDamage )
		{
			if ( IsTurn && GamemodeSystem.Instance is FreeForAll )
				GamemodeSystem.Instance.UseTurn( false );

			DamageQueue.Enqueue( info );
			HasBeenDamaged = true;
			return;
		}

		LastAttacker = info.Attacker;
		LastAttackerWeapon = info.Weapon;

		if ( Health <= 0 || LifeState != LifeState.Alive )
			return;

		Health -= info.Damage;

		if ( Health < 0 )
		{
			Health = 0;
			OnKilled();
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
