using Grubs.Equipment.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnFiring()
	{
		FireWeapon();

		if ( !ActiveGrub.Player.IsActive )
		{
			timeInState = 0f;
			currentState = BotState.Cooldown;
		}
	}


	private void FireWeapon()
	{
		//Fire the weapon depending on the type of weapon it is...
		switch ( selectedWeapon.FiringType )
		{
			case Equipment.FiringType.Instant:
				break;
			case Equipment.FiringType.Charged:
				break;
			case Equipment.FiringType.Cursor:
				break;
			case Equipment.FiringType.Complex:
				break;
			case Equipment.FiringType.Continuous:
				break;
			default:
				break;
		}
	}
}
