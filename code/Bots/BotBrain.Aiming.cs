using Grubs.Equipment.Weapons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain
{
	private void OnAiming()
	{
		// Aim at the target worm
		AimAtTarget();

		if ( IsAimedAtTarget() )
		{
			TimeInState = 0f;
			currentState = BotState.Firing;
		}
	}

	private void AimAtTarget()
	{
		Vector3 direction = (targetGrub.EyePosition.Position - ActiveGrub.EyePosition.Position).Normal;
		//ActiveGrub.PlayerController.LookInput = direction.z;
	}

	private bool IsAimedAtTarget()
	{
		Vector3 direction = (targetGrub.EyePosition.Position - ActiveGrub.EyePosition.Position).Normal;
		Vector3 lookDirection = ActiveGrub.PlayerController.LookAngles.ToRotation().Forward;
		return Vector3.GetAngle( direction, lookDirection ) < 15f;
	}
}
