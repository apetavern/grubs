using Grubs.Common;
using Grubs.UI;

namespace Grubs.Helpers;

[Title( "Grubs - World Popup Helper" ), Category( "World" )]
public sealed class WorldPopupHelper : Component
{
	public static WorldPopupHelper Local { get; private set; }

	[Property] public GameObject DamageNumberPrefab { get; set; }

	public WorldPopupHelper()
	{
		Local = this;
	}

	[Broadcast]
	public void CreateDamagePopup( Guid healthIdent, float damageTaken )
	{
		var comp = Scene.Directory.FindComponentByGuid( healthIdent );
		if ( comp is not Health health )
			return;

		if ( damageTaken == 0 )
			return;

		var popupPrefab = DamageNumberPrefab.Clone();
		var damageNumber = popupPrefab.Components.Get<DamageNumber>();
		damageNumber.Target = health.GameObject;
		damageNumber.Damage = damageTaken;
	}
}
