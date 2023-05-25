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

		/*if ( activeGrub.ActiveWeapon != null && activeGrub.ActiveWeapon.CurrentUses < activeGrub.ActiveWeapon.Charges )
		{
			Brain.PreviousState();
		}*/

		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 3f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 90f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		//DebugOverlay.TraceResult( tr );
		//DebugOverlay.TraceResult( clifftr );

		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );

		//DebugOverlay.Line( activeGrub.EyePosition + forwardLook * 50f, activeGrub.EyePosition );

		bool OnEdge = !clifftr.Hit;

		if ( Brain.TimeSinceStateStarted > 5f )
		{
			GamemodeSystem.Instance.UseTurn();
		}

		if ( (distance < 200f && !OnEdge && !lineOfSight) || !facingTarget )
		{

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

			if ( Game.Random.Float() > 0.95f )
			{
				MyPlayer.MoveInput = -MyPlayer.MoveInput;
				Input.SetAction( "backflip", true );
			}
			else
			{
				Input.SetAction( "backflip", false );
			}
		}
		else if ( OnEdge )
		{
			//MyPlayer.Inventory.SetActiveWeapon( MyPlayer.Inventory.Weapons.Where( W => W.Name.ToLower().Contains( "bazook" ) ).First(), true );
			if ( Game.Random.Float() > 0.95f )
			{
				MyPlayer.MoveInput = -activeGrub.Facing;
				Input.SetAction( "backflip", true );
			}

			MyPlayer.LookInput = Vector3.Dot( forwardLook, direction.Normal * Rotation.FromPitch( 95f ) );

			MyPlayer.MoveInput = 0f;

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
