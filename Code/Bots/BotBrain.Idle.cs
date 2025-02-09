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
		if ( ActiveGrub.Owner.IsActive )
		{
			currentState = BotState.Targeting;
		}
	}
}
