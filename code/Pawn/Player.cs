using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;
using System.Linq;

namespace TerryForm.Pawn
{
	public partial class Player : Sandbox.Player
	{
		public List<Worm> Worms { get; set; } = new();
		[Net] public Worm ActiveWorm { get; set; }
		public Client ClientOwner { get; set; }
		[Net] public long ClientId { get; set; }
		[Net] public bool IsAlive { get; set; }

		public Player() { }

		public Player( Client cl ) : this()
		{
			IsAlive = true;

			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Respawn();
				worm.DressFromClient( cl );

				Worms.Add( worm );
			}

			InitializeFromClient( cl );

			// Network this entity.
			Transmit = TransmitType.Always;
		}

		public override void Respawn()
		{
			Camera = new Camera();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			Worms.ForEach( worm => worm.Simulate( cl ) );
		}

		protected void InitializeFromClient( Client cl )
		{
			ClientOwner = cl;
			ClientId = cl.PlayerId;

			PickNextWorm();
		}

		public void OnTurnStart()
		{
			PickNextWorm();
			ActiveWorm?.OnTurnStarted();

			Log.Info( $"🐛 {ClientOwner.Name}'s turn has started using worm {ActiveWorm}." );
		}

		public void OnTurnEnd()
		{
			ActiveWorm?.OnTurnEnded();

			// Iterate through Worms to check if any are alive.
			var anyWormAlive = false;
			foreach ( var worm in Worms )
			{
				if ( worm.IsAlive )
					anyWormAlive = true;
			}

			// If all are dead, Player is also dead.
			if ( !anyWormAlive )
				IsAlive = false;

			Log.Info( $"🐛 {ClientOwner.Name}'s turn for worm {ActiveWorm} has ended." );
		}

		private void RotateWorms()
		{
			var current = Worms[0];

			Worms.RemoveAt( 0 );
			Worms.Add( current );
		}

		public void PickNextWorm()
		{
			RotateWorms();
			ActiveWorm = Worms[0];
		}

		[ClientRpc]
		public void UpdateCameraTarget( Entity target )
		{
			(Camera as Pawn.Camera).SetLookTarget( target );
		}
	}
}
