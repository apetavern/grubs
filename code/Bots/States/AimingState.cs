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

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 3f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;


		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;


		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );

		//DebugOverlay.TraceResult( tr );

		Log.Info( Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal ) );

		MyPlayer.LookInput = Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal );

		if ( !facingTarget )
		{
			MyPlayer.MoveInput = MathF.Sign( direction.Normal.x * 2f );
		}

		if ( MyPlayer.ActiveGrub.Position.x.AlmostEqual( Brain.TargetGrub.Position.x, 40f ) )
		{
			MyPlayer.MoveInput = MathF.Sign( -direction.Normal.x * 2f );
		}

		if ( Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal ) < 0.05f && Vector3.Dot( forwardLook * Rotation.FromPitch( 90f * MyPlayer.ActiveGrub.Facing ), direction.Normal ) > -0.05f )
		{
			MyPlayer.LookInput = 0f;
			FinishedState();
		}
	}
}
