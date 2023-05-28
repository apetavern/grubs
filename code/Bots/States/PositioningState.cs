using GridAStar;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots.States;

public partial class PositioningState : BaseState
{
	List<Cell> CellPath = new List<Cell>();

	public int PathIndex;

	public override void Simulate()
	{
		base.Simulate();
		DoPositioning( MyPlayer.ActiveGrub );
	}

	public Vector3 ProcessPath()
	{
		if ( Vector3.DistanceBetween( MyPlayer.ActiveGrub.Position, CellPath.ElementAt( PathIndex ).Position ) < 25f && PathIndex < CellPath.Count - 1 )
		{
			PathIndex++;
		}

		/*for ( int i = 0; i < CellPath.Count - 1; i++ )
		{
			DebugOverlay.Sphere( CellPath[i].Position, 10f, Color.Blue, 0, false );
		}*/

		return MyPlayer.ActiveGrub.Position - CellPath.ElementAt( PathIndex ).Position;
	}

	public void DoPositioning( Grub activeGrub )
	{
		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		Vector3 pathDirection = activeGrub.Position - Brain.TargetGrub.Position;

		if ( Grid.Main.Exists() )
		{
			if ( CellPath.Count == 0 )
			{
				var CellStart = Grid.Main.GetNearestCell( activeGrub.Position, false );
				var CellEnd = Grid.Main.GetNearestCell( Brain.TargetGrub.Position, false );

				CellPath = Grid.Main.ComputePath( CellStart, CellEnd, false, null ).ToList();
			}
			else if ( CellPath.Count > 1 )
			{
				pathDirection = ProcessPath();
			}
			else
			{
				var CellPos = Grid.Main.GetCellInDirection( Grid.Main.GetNearestCell( activeGrub.Position, false ), direction, 5 ).Position;
				pathDirection = MyPlayer.ActiveGrub.Position - CellPos;
				//DebugOverlay.Sphere( CellPos, 10f, Color.Blue, 0, false );
			}
		}

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 3f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 90f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		//DebugOverlay.TraceResult( tr );
		//DebugOverlay.TraceResult( clifftr );

		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );

		//DebugOverlay.Line( activeGrub.EyePosition + pathDirection, activeGrub.EyePosition );

		bool OnEdge = !clifftr.Hit;

		float LookAtTargetValue = Vector3.Dot( forwardLook, direction.Normal * Rotation.FromPitch( 90f ) );

		if ( (distance > 200f && !OnEdge && !lineOfSight) || !facingTarget )
		{

			MyPlayer.MoveInput = MathF.Sign( pathDirection.Normal.x * 2f );

			if ( MyPlayer.ActiveGrub.Position.x.AlmostEqual( Brain.TargetGrub.Position.x, 50f ) )
			{
				MyPlayer.MoveInput = MathF.Sign( -pathDirection.Normal.x * 2f );
			}

			MyPlayer.LookInput = 0f;

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.95f && !OnEdge )
			{
				Input.SetAction( "jump", true );
			}
			else
			{
				Input.SetAction( "jump", false );
			}

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.99f )
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
			if ( Game.Random.Float() > 0.95f )
			{
				MyPlayer.MoveInput = -activeGrub.Facing;
				Input.SetAction( "backflip", true );
			}

			MyPlayer.LookInput = LookAtTargetValue;

			MyPlayer.MoveInput = 0f;

		}
		else if ( distance < 30f )
		{
			MyPlayer.MoveInput = MathF.Sign( -pathDirection.Normal.x * 2f );

			MyPlayer.LookInput = 0f;

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.95f && !OnEdge )
			{
				Input.SetAction( "jump", true );
			}
			else
			{
				Input.SetAction( "jump", false );
			}

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.99f )
			{
				MyPlayer.MoveInput = -MyPlayer.MoveInput;
				Input.SetAction( "backflip", true );
			}
			else
			{
				Input.SetAction( "backflip", false );
			}
		}
		else if ( distance < 200f && distance > 30f )
		{
			MyPlayer.LookInput = LookAtTargetValue;

			MyPlayer.MoveInput = 0f;

			FinishedState();
		}

	}

	public override void FinishedState()
	{
		base.FinishedState();

		CellPath.Clear();

		MyPlayer.LookInput = 0f;

		Input.SetAction( "jump", false );

		Input.SetAction( "backflip", false );

		MyPlayer.MoveInput = 0f;
	}
}
