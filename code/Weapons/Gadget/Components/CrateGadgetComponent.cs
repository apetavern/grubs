namespace Grubs;

[Prefab]
public partial class CrateGadgetComponent : GadgetComponent
{
	[Prefab]
	public CrateType CrateType { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string SpawnSound { get; set; }

	[Prefab, ResourceType( "sound" )]
	public string PickupSound { get; set; }

	private readonly Material _spawnMaterial = Material.Load( "materials/effects/teleport/teleport.vmat" );
	private AnimatedEntity _parachute;
	private TimeSince TimeSinceSpawned { get; set; } = 0f;

	public override void Spawn()
	{
		Gadget.EnableTouch = true;
		Gadget.EnableAllCollisions = true;
		Gadget.Tags.Add( "trigger" );
		Gadget.SetMaterialOverride( _spawnMaterial );
		Gadget.PlaySound( SpawnSound );

		_ = SpawnParachuteDelayed();
	}

	private async Task SpawnParachuteDelayed()
	{
		await GameTask.DelaySeconds( 0.2f );
		_parachute = new AnimatedEntity( "models/crates/crate_parachute/crate_parachute.vmdl", Entity );
	}

	public override void Touch( Entity other )
	{
		if ( Game.IsClient || other is not Grub grub || grub.LifeState != LifeState.Alive )
			return;

		Gadget.PlayScreenSound( PickupSound );

		switch ( CrateType )
		{
			case CrateType.Weapons:
			case CrateType.Tools:
				var weaponResourcePath = CrateType == CrateType.Weapons ? CrateDrops.GetRandomWeaponFromCrate() : CrateDrops.GetRandomToolFromCrate();
				if ( !PrefabLibrary.TrySpawn<Weapon>( weaponResourcePath, out var weapon ) )
					return;

				UI.TextChat.AddInfoChatEntry( $"{grub.Player.Client.Name} picked up some goods." );
				DisplayPickupPanel( To.Single( grub.Player ), Gadget.Position, weapon.Icon );
				grub.Player.Inventory.Add( weapon );

				Gadget.Delete();
				break;
			case CrateType.Health:
				grub.Health += 25;
				UI.TextChat.AddInfoChatEntry( $"{grub.Player.Client.Name} received medical attention." );
				HealGrubEventClient( To.Everyone, grub, 25 );
				Gadget.Delete();
				break;
			default:
				return;
		}
	}

	public override void Simulate( IClient client )
	{
		if ( Game.IsServer )
		{
			if ( TimeSinceSpawned > 1f )
				Gadget.ClearMaterialOverride();
		}

		var helper = new MoveHelper( Gadget.Position, Gadget.Velocity );
		helper.Trace = helper.Trace
			.Size( Gadget.CollisionBounds )
			.Ignore( Grub )
			.Ignore( Gadget )
			.WithAnyTags( "player", "solid", "gadget" )
			.WithoutTags( "dead" );

		var groundEntity = helper.TraceDirection( Vector3.Down ).Entity;

		if ( groundEntity is null )
		{
			helper.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			_parachute?.DeleteAsync( 0.3f );
			_parachute?.SetAnimParameter( "landed", true );
			_parachute = null;
			helper.Velocity = 0;
		}

		var parachuteAirFrictionModifier = _parachute is not null ? 1.5f : 0.5f;
		helper.ApplyFriction( 2.0f * parachuteAirFrictionModifier, Time.Delta );
		helper.TryMove( Time.Delta );

		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;
		if ( Game.IsServer )
		{
			Gadget.LocalRotation = Rotation.Slerp( Gadget.Rotation, Rotation.Identity *
				new Angles( _parachute is not null ? MathF.Sin( Time.Now * 2f ) * 15f : 0f, 0, 0 ).ToRotation(), 0.75f );
		}

	}

	[ClientRpc]
	private void DisplayPickupPanel( Vector3 pos, string icon )
	{
		_ = new UI.CratePickupWorldPanel( pos, icon );
	}

	[ClientRpc]
	private void HealGrubEventClient( Grub grub, int healAmount )
	{
		Event.Run( GrubsEvent.Grub.Healed, grub.NetworkIdent, healAmount );
	}

	public static Gadget SpawnCrate( CrateType crate )
	{
		var path = crate switch
		{
			CrateType.Weapons => "prefabs/crates/weapons_crate.prefab",
			CrateType.Tools => "prefabs/crates/tools_crate.prefab",
			CrateType.Health => "prefabs/crates/health_crate.prefab",
			_ => "",
		};

		return PrefabLibrary.TrySpawn<Gadget>( path, out var drop ) ? drop : null;
	}

	[ConCmd.Admin( "gr_spawn_crate" )]
	private static void DebugSpawnCrate()
	{
		var crate = SpawnCrate( (CrateType)Random.Shared.Int( 0, 2 ) );
		var player = ConsoleSystem.Caller.Pawn as Player;
		player?.Gadgets.Add( crate );
		crate.Owner = player;
		crate.Position = player.Grubs.FirstOrDefault()?.Position + Vector3.Up * 300f ?? Vector3.Zero;
	}
}

public enum CrateType
{
	Weapons,
	Tools,
	Health
}
