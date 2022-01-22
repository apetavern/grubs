using Sandbox;
using System;
using System.Linq;
using Grubs.Weapons;
using System.Collections.Generic;

namespace Grubs.Pawn
{
	public partial class PlayerInventory : BaseNetworkable
	{
		public Entity Owner { get; set; }
		[Net] public List<Weapon> Items { get; set; } = new();

		public void Add( Entity ent, bool makeActive = false )
		{
			var weapon = ent as Weapon;

			// If this weapon is a pick-up, add ammo to player's existing weapon of the same type.
			if ( weapon != null && IsCarryingType( ent.GetType() ) )
			{
				var existingWeapon = Items.Where( x => x.GetType() == weapon.GetType() ).FirstOrDefault();
				if ( existingWeapon.Ammo != -1 ) existingWeapon.Ammo++;

				ent.Delete();

				return;
			}

			Items.Add( ent as Weapon );

			ent.Parent = Owner;
			ent.OnCarryStart( Owner );
		}

		[ServerCmd]
		public static void EquipItemFromIndex( int itemIndex )
		{
			var player = ConsoleSystem.Caller.Pawn as Pawn.Player;

			var activeWorm = player.ActiveWorm;

			if ( activeWorm is null || !activeWorm.IsCurrentTurn )
				return;

			var inventory = player.PlayerInventory;

			if ( inventory.Items[itemIndex] is null )
				return;

			activeWorm.EquipWeapon( inventory.Items[itemIndex] );
		}

		public bool IsCarryingType( Type t )
		{
			return Items.Any( x => x.GetType() == t );
		}

		public bool HasAmmo( int itemIndex )
		{
			return Items[itemIndex].Ammo != 0;
		}
	}
}
