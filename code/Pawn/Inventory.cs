using Sandbox;
using System;
using System.Linq;
using TerryForm.Weapons;

namespace TerryForm.Pawn
{
	public class Inventory : BaseInventory
	{
		public Inventory( Player player ) : base( player )
		{

		}

		public override bool Add( Entity ent, bool makeActive = false )
		{
			var player = Owner as Player;
			var weapon = ent as Weapon;

			// If this weapon is a pick-up, add ammo to player's existing weapon of the same type.
			if ( weapon != null && IsCarryingType( ent.GetType() ) )
			{
				var existingWeapon = (Weapon)List.Where( x => x.GetType() == weapon.GetType() ).FirstOrDefault();
				if ( existingWeapon.Ammo != -1 ) existingWeapon.Ammo++;

				ent.Delete();
			}

			return base.Add( ent, makeActive );
		}

		public override void OnChildRemoved( Entity child )
		{
			/* 
			 * Do nothing, the default behaviour will remove an item from the inventory list 
			 * if the Parent of the item changes, which it does because we give it a worm.
			 */
		}

		[ServerCmd]
		public static void EquipItemFromIndex( int itemIndex )
		{
			var player = ConsoleSystem.Caller.Pawn as Pawn.Player;

			var activeWorm = player.ActiveWorm;

			if ( activeWorm is null || !activeWorm.IsCurrentTurn )
				return;

			var inventory = player.Inventory;

			if ( inventory.GetSlot( itemIndex ) is null )
				return;

			activeWorm.EquipWeapon( inventory.GetSlot( itemIndex ) as Weapon );
		}

		public bool IsCarryingType( Type t )
		{
			return List.Any( x => x.GetType() == t );
		}

		public bool HasAmmo( int itemIndex )
		{
			return (List[itemIndex] as Weapon).Ammo != 0;
		}
	}
}
