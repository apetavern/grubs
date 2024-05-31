using Grubs.UI;

namespace Grubs.Helpers;

[Title( "Grubs - World Popup Helper" ), Category( "World" )]
public sealed class WorldPopupHelper : Component
{
	public static WorldPopupHelper Instance { get; private set; }

	[Property] public GameObject DamageNumberPrefab { get; set; }
	[Property] public GameObject CratePickupPrefab { get; set; }

	public WorldPopupHelper()
	{
		Instance = this;
	}

	[Broadcast]
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

	[Broadcast]
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
}
