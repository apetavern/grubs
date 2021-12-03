using Sandbox;
using TerryForm.Utils;
using System.Linq;
using System.Threading.Tasks;
using TerryForm.UI;

namespace TerryForm.States.SubStates
{
	public partial class Turn : BaseState
	{
		public override string StateName => "TURN";
		public override int StateDurationSeconds => GameConfig.TurnDurationSeconds;
		public static Turn Instance { get; set; }
		private TimeSince TimeSinceResolutionStageStarted { get; set; }
		private PlayingState PlayingState { get; set; }
		[Net] public Pawn.Player ActivePlayer { get; set; }
		[Net] public float WindForce { get; set; }

		public Turn()
		{
			Instance = this;
		}

		public Turn InitFrom( Pawn.Player player, PlayingState playState )
		{
			ActivePlayer = player;
			PlayingState = playState;

			return this;
		}

		protected override void OnStart()
		{
			base.OnStart();

			UpdateWind();

			// Let the player know that their turn has started.
			ActivePlayer?.OnTurnStart();

			ChatBox.AddInformation( To.Everyone, $"{ActivePlayer.ActiveWorm.Name}'s turn has started.", $"avatar:{ActivePlayer.Client.PlayerId}" );

			// Update camera target for all players.
			StateHandler.Instance?.Players?.ForEach( player => player.UpdateCameraTarget( ActivePlayer.ActiveWorm ) );
		}

		private void UpdateWind()
		{
			WindForce = Rand.Float( -1, 1 );

			// Let the HUD know that the wind has changed.
			HudEntity.UpdateWind( WindForce );
		}

		protected override void OnTimeUp()
		{
			OnFinish();
		}

		protected override async void OnFinish()
		{
			base.OnFinish();

			// Let the player know that their turn has ended, useful to kill their ActiveWorm.
			ActivePlayer?.OnTurnEnd();

			// Wait for all entities with a velocity to stop moving.
			await CheckResolvedAsync();

			// Let the playing state know this turn has ended so that it can start another.
			PlayingState?.OnTurnFinished();
		}

		private async Task<bool> CheckResolvedAsync()
		{
			TimeSinceResolutionStageStarted = 0;
			var allResolved = false;

			while ( allResolved == false && TimeSinceResolutionStageStarted < 30 )
			{
				Log.Info( $"❤️ Waiting entities to resolve for {TimeSinceResolutionStageStarted} seconds." );
				await GameTask.DelaySeconds( 1 );

				allResolved = !Entity.All.OfType<IAwaitResolution>().Any( ent => !ent.IsResolved );
			}

			Log.Info( "❤️ All entities are resolved." );
			return true;
		}

		public void ForceEnd()
		{
			OnFinish();
		}

		// Debug method for ending a turn immediately.
		[ServerCmd( "tf_turn_end" )]
		public static void EndTurn()
		{
			(StateHandler.Instance?.State as PlayingState)?.Turn?.OnFinish();
		}
	}
}
