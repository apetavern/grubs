using Grubs.UI;

namespace Grubs.Helpers;

[Title( "Grubs - World Popup Helper" ), Category( "World" )]
public sealed class WorldPopupHelper : Component
{
	private static readonly Logger Log = new("WorldPopupHelper");
	public static WorldPopupHelper Instance { get; private set; }

	[Property] public GameObject DamageNumberPrefab { get; set; }
	[Property] public GameObject CratePickupPrefab { get; set; }
	[Property] public GameObject KillZoneDeathIndicatorPrefab { get; set; }

	public WorldPopupHelper()
	{
		Instance = this;
	}

	[Rpc.Broadcast]
	public void CreateDamagePopup( Guid targetIdent, float damageTaken )
	{
		var target = Scene.Directory.FindByGuid( targetIdent );
		if ( damageTaken == 0 )
			return;

		var popupPrefab = DamageNumberPrefab.Clone();
		var damageNumber = popupPrefab.Components.Get<DamageNumber>();
		damageNumber.Target = target;
		damageNumber.Damage = damageTaken;
	}

	[Rpc.Broadcast]
	public void CreatePickupPopup( Guid targetIdent, string icon )
	{
		var target = Scene.Directory.FindByGuid( targetIdent );
		if ( icon == null )
			return;

		var popupPrefab = CratePickupPrefab.Clone();
		var cratePickup = popupPrefab.Components.Get<CratePickup>();
		cratePickup.Target = target;
		cratePickup.Icon = icon;
	}

	[Rpc.Broadcast]
	public void CreateKillZoneDeathIndicator( Vector3 worldPosition )
	{
		Log.Info( $"Creating kill zone death indicator at {worldPosition}." );

		var safePosition = worldPosition.WithZ( 20f );
		var target = new GameObject { WorldPosition = safePosition };
		
		var go = KillZoneDeathIndicatorPrefab.Clone();
		go.SetParent( target );
		var indicator = go.Components.Get<KillZoneDeathIndicator>();
		indicator.Target = target;
	}
}
