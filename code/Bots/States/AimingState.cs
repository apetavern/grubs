using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;
public partial class AimingState : BaseState
{

	new public float MaxTimeInState = 3f;

	public override void Simulate()
	{
		base.Simulate();
		AimAtTarget();
	}

	public void AimAtTarget()
	{
		var activeGrub = MyPlayer.ActiveGrub;

		MyPlayer.MoveInput = 0f;

		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );

		float LookAtTargetValue = Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal );

		//DebugOverlay.TraceResult( tr );

		MyPlayer.LookInput = LookAtTargetValue;

		if ( !facingTarget )
		{
			MyPlayer.MoveInput = MathF.Sign( direction.Normal.x * 2f );
		}

		if ( MyPlayer.ActiveGrub.Position.x.AlmostEqual( Brain.TargetGrub.Position.x, 40f ) )
		{
			MyPlayer.MoveInput = MathF.Sign( -direction.Normal.x * 2f );
		}

		if ( LookAtTargetValue < 0.05f && LookAtTargetValue > -0.05f )
		{
			MyPlayer.LookInput = 0f;
			FinishedState();
		}
	}
}
