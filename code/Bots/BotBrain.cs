using Grubs.Bots.States;
using Grubs.Bots;
using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Bots;
public partial class BotBrain : Entity
{
	public Grub TargetGrub;

	public int CurrentState = 0;

	public IEnumerable<BaseState> States;

	public const float MaxFallDistance = 150f;

	public Player MyPlayer => Owner as Player;

	public TimeSince TimeSinceStateStarted = 0f;

	public override void Spawn()
	{
		Components.Create<TargetingState>();
		Components.Create<CrateFindingState>();
		Components.Create<PositioningState>();
		Components.Create<WeaponSelectState>();
		Components.Create<AimingState>();
		Components.Create<FiringState>();
		Components.Create<RetreatState>();

		Components.Create<BaseState>();

		States = Components.GetAll<BaseState>();

		if ( GamemodeSystem.Instance.Terrain is not Terrain terrain )
			return;

		BBox worldbox = new BBox();
		worldbox.Maxs = new Vector3( terrain.WorldTextureLength / 2f, 10f, terrain.WorldTextureHeight );
		worldbox.Mins = new Vector3( -terrain.WorldTextureLength / 2f, -10f, -terrain.WorldTextureHeight );

		if ( !GridAStar.Grid.Exists() )
		{
			GridAStar.Grid.Create( Vector3.Zero, worldbox, Rotation.Identity, worldOnly: false, heightClearance: 30f, stepSize: 100f, standableAngle: 50f, save: false );
		}
	}

	public void SimulateCurrentState()
	{
		if ( MyPlayer.IsTurn && CurrentState < States.Count() - 1 && !MyPlayer.ActiveGrub.HasBeenDamaged )
		{
			if ( TimeSinceStateStarted > States.ElementAt( CurrentState ).MaxTimeInState && !States.ElementAt( CurrentState ).ToString().Contains( "Base" ) )
			{
				States.ElementAt( CurrentState ).FinishedState();
				return;
			}
			States.ElementAt( CurrentState ).Simulate();

			(States.ElementAt( 0 ) as TargetingState).LineOfSightTargetCheck();

			DebugOverlay.Text( States.ElementAt( CurrentState ).ToString(), MyPlayer.ActiveGrub.EyePosition );
		}
		else
		{
			CurrentState = 0;
			TargetGrub = null;
		}
	}

	public void PreviousState()
	{
		if ( States.Count() > CurrentState + 1 )
		{
			CurrentState -= 1;
			TimeSinceStateStarted = 0f;

			Input.SetAction( "jump", false );
			Input.SetAction( "backflip", false );
			Input.SetAction( "fire", false );

			//Log.Info( "Previous State!" );
		}
		else
		{
			//Log.Info( "No more states!" );
		}
	}

	public void NextState()
	{
		if ( States.Count() > CurrentState + 1 )
		{
			CurrentState += 1;
			TimeSinceStateStarted = 0f;

			Input.SetAction( "jump", false );
			Input.SetAction( "backflip", false );
			Input.SetAction( "fire", false );

			//Log.Info( "Next State!" );
		}
		else
		{
			//Log.Info( "No more states!" );
		}
	}
}
