using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnMoving()
	{
		if ( targetPosition == Vector3.Zero )
		{
			targetPosition = CalculateBestPosition();
		}
		else
		{
			MoveTowardsTarget();
		}

		if ( HasReachedPosition( targetPosition ) )
		{
			timeInState = 0f;
			currentState = BotState.SelectingWeapon;
		}
	}

	private void MoveTowardsTarget()
	{
		//ActiveGrub.PlayerController.MoveInput = (ActiveGrub.Transform.Position - targetPosition).Normal.x;
		//Needs advanced pathing shit, some jumping, BACKFLIPS would be amazing but I doubt we can do that...
	}

	private Vector3 CalculateBestPosition()
	{
		// Gotta check for line of sight later
		return Scene.NavMesh.GetClosestPoint( targetGrub.Transform.Position ).Value;
	}

	private bool HasReachedPosition( Vector3 position )
	{
		return Vector3.DistanceBetween( ActiveGrub.Transform.Position, position ) < 100f;
	}
}
