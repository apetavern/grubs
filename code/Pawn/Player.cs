using Sandbox;
using System.Linq;
using System.Collections.Generic;
using TerryForm.Utils;
using TerryForm.Weapons;

namespace TerryForm.Pawn
{
	public partial class Player : Sandbox.Player
	{
		[Net] public List<Worm> Worms { get; set; } = new();
		[Net] public Worm ActiveWorm { get; set; }
		[Net] public bool IsAlive { get; set; }

		public Player()
		{
			Inventory = new Inventory( this );
		}

		public Player( Client cl ) : this()
		{
			IsAlive = true;

			// Create worms
			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Owner = this;
				worm.Respawn();
				worm.DressFromClient( cl );

				Worms.Add( worm );
			}

			ReceiveLoadout();
			Initialize();
		}

		private void ReceiveLoadout()
		{
			var weapons = Library.GetAll<Weapon>()
				.Where( weapon => !weapon.IsAbstract );

			foreach ( var weapon in weapons )
			{
				Inventory.Add( Library.Create<Weapon>( weapon ) );
			}
		}

		public override void Respawn()
		{
			Camera = new Camera();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			foreach ( var worm in Worms )
				worm.Simulate( cl );
		}

		protected void Initialize()
		{
			PickNextWorm();

			// Set the initial target of this players camera. This will be overriden later when the turn changes.
			UpdateCameraTarget( ActiveWorm );
		}

		public void OnTurnStart()
		{
			PickNextWorm();
			ActiveWorm?.OnTurnStarted();

			Log.Info( $"🐛 {Client.Name}'s turn has started using worm {ActiveWorm}." );
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

			Log.Info( $"🐛 {Client.Name}'s turn for worm {ActiveWorm} has ended." );
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
