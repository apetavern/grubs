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
		if ( Vector3.DistanceBetween( MyPlayer.ActiveGrub.Position, CellPath.ElementAt( PathIndex ).Position ) < 50f && PathIndex < CellPath.Count - 1 )
		{
			PathIndex++;
		}
		else
		{
			CellPath.Clear();
			PathIndex = 0;
			return Vector3.Zero;
		}

		if ( BotBrain.Debug )
		{
			for ( int i = 0; i < CellPath.Count - 1; i++ )
			{
				DebugOverlay.Sphere( CellPath[i].Position, 10f, Color.Blue, 0, false );
			}
		}

		return MyPlayer.ActiveGrub.Position - CellPath.ElementAt( PathIndex ).Position;
	}

	public void DoPositioning( Grub activeGrub )
	{
		Vector3 direction = activeGrub.Position - Brain.TargetGrub.Position;

		Vector3 pathDirection = activeGrub.Position - Brain.TargetGrub.Position;

		Grid grid = Grid.Grids.First().Value;


		if ( CellPath.Count == 0 )
		{
			var CellStart = grid.GetNearestCell( activeGrub.Position, false );
			var CellEnd = grid.GetNearestCell( Brain.TargetGrub.Position, false );

			//DebugOverlay.Sphere( CellStart.Position, 10f, Color.Blue, 0, false );

			//DebugOverlay.Sphere( CellEnd.Position, 10f, Color.Blue, 0, false );

			CellPath = grid.ComputePath( CellStart, CellEnd, false, null ).ToList();
		}
		else if ( CellPath.Count > 1 )
		{
			pathDirection = ProcessPath();
		}

		float distance = direction.Length;

		var tr = Trace.Ray( activeGrub.EyePosition - Vector3.Up, Brain.TargetGrub.EyePosition - Vector3.Up * 3f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		bool lineOfSight = tr.Entity == Brain.TargetGrub;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		bool facingTarget = Vector3.DistanceBetween( activeGrub.Position + activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position ) < Vector3.DistanceBetween( activeGrub.Position - activeGrub.Rotation.Forward * 20f, Brain.TargetGrub.Position );


		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 15f + Vector3.Up * 5f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 512f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		var ceiltr = Trace.Ray( activeGrub.EyePosition, activeGrub.EyePosition + Vector3.Up * 10f ).Ignore( activeGrub ).UseHitboxes( true ).Run();


		bool OnEdge = clifftr.Distance > BotBrain.MaxFallDistance || !clifftr.Hit || MathF.Round( clifftr.EndPosition.z ) == 0;

		float LookAtTargetValue = Vector3.Dot( forwardLook, direction.Normal * Rotation.FromPitch( 90f ) );

		if ( BotBrain.Debug )
		{
			DebugOverlay.Line( activeGrub.EyePosition + pathDirection, activeGrub.EyePosition );
			DebugOverlay.TraceResult( tr );
			DebugOverlay.TraceResult( clifftr );
		}

		if ( (distance > 200f && !OnEdge && !lineOfSight) || !facingTarget )
		{

			MyPlayer.MoveInput = MathF.Sign( pathDirection.Normal.x * 2f );

			if ( MyPlayer.ActiveGrub.Position.x.AlmostEqual( Brain.TargetGrub.Position.x, 75f ) )
			{
				MyPlayer.MoveInput = MathF.Sign( -pathDirection.Normal.x * 2f );
			}

			MyPlayer.LookInput = 0f;

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.95f && !OnEdge && !ceiltr.Hit )
			{
				Input.SetAction( "jump", true );
			}
			else
			{
				Input.SetAction( "jump", false );
			}

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.99f && !ceiltr.Hit )
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

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.95f && !OnEdge && !ceiltr.Hit )
			{
				Input.SetAction( "jump", true );
			}
			else
			{
				Input.SetAction( "jump", false );
			}

			if ( MathF.Round( Time.Now ) % 5 == 1 && Game.Random.Float() > 0.99f && !ceiltr.Hit )
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

		PathIndex = 0;

		Input.SetAction( "jump", false );

		Input.SetAction( "backflip", false );

		MyPlayer.MoveInput = 0f;
	}
}
