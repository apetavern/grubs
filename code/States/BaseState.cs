using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States
{
	public abstract partial class BaseState : BaseNetworkable
	{
		public virtual string StateName => "";
		public virtual int StateDurationSeconds { get; protected set; } = 0;
		[Net] public float StateEndTime { get; set; }

		public float TimeLeft
		{
			get
			{
				return StateEndTime - Time.Now;
			}
		}

		public void Start()
		{
			Log.Info( $"🟢 {StateName} state started" );

			if ( StateDurationSeconds > 0 )
			{
				StateEndTime = Time.Now + StateDurationSeconds;
			}

			OnStart();
		}

		public void Finish()
		{
			Log.Info( $"🔴 {StateName} state ended." );

			StateEndTime = 0f;

			OnFinish();
		}

		public void AddPlayer( Pawn.Player player )
		{
			Host.AssertServer();

			if ( !StateHandler.Instance.Players.Contains( player ) )
				StateHandler.Instance.Players.Add( player );
		}

		public void RotatePlayers()
		{
			Host.AssertServer();

			var current = StateHandler.Instance?.Players[0];
			StateHandler.Instance?.Players.RemoveAt( 0 );
			StateHandler.Instance?.Players.Add( current );

			SkipDeadPlayer();
		}

		private void SkipDeadPlayer()
		{
			var current = StateHandler.Instance?.Players[0];
			if ( !current.IsAlive )
				RotatePlayers();
		}

		public void SetTimeRemaining( int newDuration )
		{
			Host.AssertServer();
			StateEndTime = Time.Now + newDuration;
		}

		public virtual void OnPlayerSpawn( Pawn.Player player ) { }

		public virtual void OnPlayerJoin( Pawn.Player player )
		{
			AddPlayer( player );
		}

		public virtual void OnPlayerLeave( Pawn.Player player ) { }

		public virtual void OnTick()
		{
			if ( StateEndTime > 0f && Time.Now >= StateEndTime )
			{
				StateEndTime = 0f;
				OnTimeUp();
			}
		}

		protected virtual void OnStart() { }

		protected virtual void OnFinish() { }

		protected virtual void OnTimeUp() { }
	}
}
