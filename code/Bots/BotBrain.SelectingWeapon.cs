using Grubs.Equipment.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnSelectingWeapon()
	{
		if ( selectedWeapon == null )
		{
			selectedWeapon = SelectBestWeapon();
		}

		if ( selectedWeapon != null )
		{
			timeInState = 0f;
			currentState = BotState.Aiming;
		}
	}

	private Weapon SelectBestWeapon()
	{
		return ActiveGrub.Player.Inventory.ActiveEquipment.Components.Get<Weapon>();
	}
}
