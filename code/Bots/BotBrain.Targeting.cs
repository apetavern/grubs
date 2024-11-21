using Grubs.Pawn;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnTargeting()
	{
		targetGrub = FindTargetWorm();
		if ( targetGrub != null )
		{
			timeInState = 0f;
			currentState = BotState.Moving;
		}
	}

	private Grub FindTargetWorm()
	{
		// Example implementation: find the closest enemy worm
		var grubs = Scene.GetAllComponents<Grub>().Where( X => X.Player != ActiveGrub.Player );
		Grub closestWorm = null;
		float minDistance = float.MaxValue;

		foreach ( var grub in grubs )
		{
			float distance = Vector3.DistanceBetween( ActiveGrub.WorldPosition, grub.WorldPosition );
			if ( distance < minDistance )
			{
				minDistance = distance;
				closestWorm = grub;
			}
		}

		return closestWorm;
	}
}
