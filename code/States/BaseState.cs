using Sandbox;
using System.Collections.Generic;

namespace TerryForm.States
{
	public abstract partial class BaseState : BaseNetworkable
	{
		public virtual string StateName => "";
		public virtual int StateDuration => 0;
		public float StateEndTime { get; set; }

		public List<Player> PlayerList = new();

		public float TimeLeft
		{
			get
			{
				return StateEndTime - Time.Now;
			}
		}

		public void Start()
		{
			if ( Host.IsServer && StateDuration > 0 )
			{
				StateEndTime = Time.Now + StateDuration;
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

		public void AddPlayer( Player player )
		{
			Host.AssertServer();

			if ( !PlayerList.Contains( player ) ) PlayerList.Add( player );
		}

		public virtual void OnPlayerSpawn( Player player ) { }

		public virtual void OnPlayerJoin( Player player ) { }

		public virtual void OnPlayerLeave( Player player ) { }

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
