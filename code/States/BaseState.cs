using Sandbox;
using System.Collections.Generic;
using TerryForm.Pawn;

namespace TerryForm.States
{
	public abstract partial class BaseState : BaseNetworkable
	{
		public virtual string StateName => "";
		public virtual int StateDurationSeconds { get; protected set; } = 0;
		public float StateEndTime { get; set; }

		public List<Pawn.Player> PlayerList = new();

		public float TimeLeft
		{
			get
			{
				return StateEndTime - Time.Now;
			}
		}

		public void Start()
		{
			if ( Host.IsServer && StateDurationSeconds > 0 )
			{
				StateEndTime = Time.Now + StateDurationSeconds;
			}

			OnStart();
		}

		public void Finish()
		{
			if ( Host.IsServer )
			{
				StateEndTime = 0f;
				PlayerList.Clear();
			}

			OnFinish();
		}

		public void AddPlayer( Pawn.Player player )
		{
			Host.AssertServer();

			if ( !PlayerList.Contains( player ) ) PlayerList.Add( player );
		}

		public void RotatePlayers()
		{
			Host.AssertServer();

			var current = Game.StateHandler.Players[0];
			Game.StateHandler.Players.RemoveAt( 0 );
			Game.StateHandler.Players.Add( current );
		}

		public void SetTimeRemaining( int newDuration )
		{
			Host.AssertServer();

			StateDurationSeconds = newDuration;
			StateEndTime = Time.Now + StateDurationSeconds;
		}

		public virtual void OnPlayerSpawn( Pawn.Player player ) { }

		public virtual void OnPlayerJoin( Pawn.Player player ) { }

		public virtual void OnPlayerLeave( Pawn.Player player ) { }

		public virtual void OnTick()
		{
			if ( Host.IsServer )
			{
				if ( StateEndTime > 0f && Time.Now >= StateEndTime )
				{
					StateEndTime = 0f;
					OnTimeUp();
				}
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
