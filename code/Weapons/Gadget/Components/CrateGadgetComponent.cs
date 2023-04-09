namespace Grubs;

[Prefab]
public partial class CrateGadgetComponent : GadgetComponent
{
	[Prefab]
	public CrateType CrateType { get; set; }

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
	}

	public override void OnTouch( Entity other )
	{
		if ( Game.IsClient || other is not Grub grub )
			return;

		Gadget.PlaySound( PickupSound );

		switch ( CrateType )
		{
			case CrateType.Weapons:
				var weaponResourcePath = CrateDrops.GetRandomWeaponFromCrate();
				TextChat.AddInfoChatEntry( $"{grub.Player.Client.Name} picked up some weaponized goods." );
				grub.Player.Inventory.AddByResourcePath( weaponResourcePath );
				Gadget.Delete();
				break;
			case CrateType.Tools:
				weaponResourcePath = CrateDrops.GetRandomToolFromCrate();
				TextChat.AddInfoChatEntry( $"{grub.Player.Client.Name} picked up a tool." );
				grub.Player.Inventory.AddByResourcePath( weaponResourcePath );
				Gadget.Delete();
				break;
			case CrateType.Health:
				grub.Health += 25;
				TextChat.AddInfoChatEntry( $"{grub.Player.Client.Name} received medical attention." );
				HealGrubEventClient( To.Everyone, grub, 25 );
				Gadget.Delete();
				break;
			default:
				return;
		}
	}

	[Event.Client.Frame]
	public void UpdateEffects()
	{
		if ( TimeSinceSpawned > 2f )
			Gadget.ClearMaterialOverride();
		else
			Gadget.SceneObject.Attributes.Set( "opacity", TimeSinceSpawned / 2 );
	}

	[ClientRpc]
	public void HealGrubEventClient( Grub grub, int healAmount )
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
