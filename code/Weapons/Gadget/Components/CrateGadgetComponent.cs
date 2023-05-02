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
	private TimeSince TimeSinceSpawned { get; set; } = 0f;

	public override void Spawn()
	{
		Gadget.EnableTouch = true;
		Gadget.EnableAllCollisions = true;
		Gadget.Tags.Add( "trigger" );
		Gadget.SetMaterialOverride( _spawnMaterial );
		Gadget.PlaySound( SpawnSound );
	}

	public override void OnTouch( Entity other )
	{
		if ( Game.IsClient || other is not Grub grub )
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
				DisplayPickupPanel( To.Everyone, Gadget.Position, weapon.Icon );
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
			.WithAnyTags( "player", "solid" )
			.WithoutTags( "dead" );

		var groundEntity = helper.TraceDirection( Vector3.Down ).Entity;

		if ( groundEntity is null )
		{
			helper.Velocity += Game.PhysicsWorld.Gravity * Time.Delta;
		}
		else
		{
			helper.Velocity = 0;
		}

		helper.ApplyFriction( 2.0f, Time.Delta );
		helper.TryMove( Time.Delta );

		Gadget.Velocity = helper.Velocity;
		Gadget.Position = helper.Position;
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
