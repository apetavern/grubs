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

public partial class CrateFindingState : BaseState
{
	List<AStarNode> CellPath = new();

	public int PathIndex;

	Gadget FoundCrate;

	public override void Simulate()
	{
		base.Simulate();
		FindCrates();
		DoPositioning( MyPlayer.ActiveGrub );
	}

	public void FindCrates()
	{
		if ( FoundCrate is null )
		{
			foreach ( var crate in Gadget.All.Where( E => E.GetType() == typeof( Gadget ) && E.Components.TryGet<CrateGadgetComponent>( out CrateGadgetComponent comp ) ).OrderBy( E => Vector3.DistanceBetween( E.Position, MyPlayer.ActiveGrub.Position ) ) )
			{
				if ( Vector3.DistanceBetween( MyPlayer.ActiveGrub.Position, crate.Position ) < 200f )
				{
					FoundCrate = crate as Gadget;
				}
			}

			if ( FoundCrate is null || !FoundCrate.IsValid() )
			{
				FinishedState();
			}
		}
	}

	public Vector3 ProcessPath()
	{
		if ( Vector3.DistanceBetween( MyPlayer.ActiveGrub.Position, CellPath.ElementAt( PathIndex ).EndPosition ) < 50f && PathIndex < CellPath.Count - 1 )
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
				DebugOverlay.Sphere( CellPath[i].EndPosition, 10f, Color.Blue, 0, false );
			}
		}

		return MyPlayer.ActiveGrub.Position - CellPath.ElementAt( PathIndex ).EndPosition;
	}

	public void DoPositioning( Grub activeGrub )
	{
		if ( FoundCrate is null || !FoundCrate.IsValid() )
		{
			MyPlayer.LookInput = 0f;

			MyPlayer.MoveInput = 0f;

			if ( Brain.TimeSinceStateStarted > 4f )
			{
				FinishedState();
			}
			return;
		}

		Vector3 direction = activeGrub.Position - FoundCrate.Position;

		Vector3 pathDirection = activeGrub.Position - FoundCrate.Position;

		Grid grid = Grid.Grids.First().Value;

		if ( CellPath is null || CellPath.Count == 0 )
		{
			var CellStart = grid.GetNearestCell( activeGrub.Position, false );
			var CellEnd = grid.GetNearestCell( FoundCrate.Position, false );
			CellPath = new AStarPathBuilder( grid ).Run( CellStart, CellEnd ).Nodes;
		}
		else if ( CellPath.Count > 1 )
		{
			pathDirection = ProcessPath();
		}


		float distance = direction.Length;

		var forwardLook = activeGrub.EyeRotation.Forward * activeGrub.Facing;

		var clifftr = Trace.Ray( activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f + Vector3.Up * 5f, activeGrub.EyePosition + activeGrub.Rotation.Forward * 20f - Vector3.Up * 512f ).Ignore( activeGrub ).UseHitboxes( true ).Run();

		//DebugOverlay.TraceResult( tr );
		//DebugOverlay.Line( activeGrub.EyePosition + pathDirection, activeGrub.EyePosition );

		bool OnEdge = clifftr.Distance > BotBrain.MaxFallDistance || !clifftr.Hit || MathF.Round( clifftr.EndPosition.z ) == 0;

		float LookAtTargetValue = Vector3.Dot( forwardLook, direction.Normal * Rotation.FromPitch( 90f ) );

		if ( distance > 10f && !OnEdge )
		{
			MyPlayer.MoveInput = MathF.Sign( pathDirection.Normal.x * 2f );

			MyPlayer.LookInput = 0f;

			if ( Game.Random.Float() > 0.95f )
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
		else if ( distance < 10f )
		{
			MyPlayer.LookInput = LookAtTargetValue;

			MyPlayer.MoveInput = 0f;

			FinishedState();
		}

	}

	public override void FinishedState()
	{
		base.FinishedState();

		if ( CellPath != null )
		{
			CellPath.Clear();
		}

		MyPlayer.LookInput = 0f;

		PathIndex = 0;

		Input.SetAction( "jump", false );

		Input.SetAction( "backflip", false );

		MyPlayer.MoveInput = 0f;
	}
}
