using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnIdle()
	{
		timeInState = 0f;
		if ( ActiveGrub.Player.IsActive )
		{
			currentState = BotState.Targeting;
		}
	}
}
