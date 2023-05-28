using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;

public partial class RetreatState : BaseState
{
	public override void Simulate()
	{
		base.Simulate();
		DoPositioning( MyPlayer.ActiveGrub );
	}

	public void DoPositioning( Grub activeGrub )
	{
		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 90f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		//DebugOverlay.TraceResult( tr );
		//DebugOverlay.TraceResult( clifftr );

		//DebugOverlay.Line( activeGrub.EyePosition + forwardLook * 50f, activeGrub.EyePosition );

		bool OnEdge = !clifftr.Hit;

		if ( Brain.TimeSinceStateStarted > 5f )
		{
			GamemodeSystem.Instance.UseTurn();
		}

		MyPlayer.MoveInput = MathF.Sign( -direction.Normal.x * 2f );

		MyPlayer.LookInput = 0f;

		if ( Game.Random.Float() > 0.9f && !OnEdge )
		{
			Input.SetAction( "jump", true );
		}
		else
		{
			Input.SetAction( "jump", false );
		}

		if ( Game.Random.Float() > 0.995f )
		{
			MyPlayer.MoveInput = -MyPlayer.MoveInput;
			Input.SetAction( "backflip", true );
		}
		else
		{
			Input.SetAction( "backflip", false );
		}

	}

	public override void FinishedState()
	{
		base.FinishedState();

		MyPlayer.LookInput = 0f;

		Input.SetAction( "jump", false );

		Input.SetAction( "backflip", false );

		MyPlayer.MoveInput = 0f;
	}
}
