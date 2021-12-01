using Sandbox;
using System.Linq;
using System.Collections.Generic;
using TerryForm.Utils;
using TerryForm.Weapons;
using TerryForm.States;

namespace TerryForm.Pawn
{
	public partial class Player : Sandbox.Player
	{
		[Net] public List<Worm> Worms { get; set; } = new();
		[Net] public Worm ActiveWorm { get; set; }

		public Player()
		{
			Inventory = new Inventory( this );
		}

		public Player( Client cl ) : this()
		{
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
				if ( worm.LifeState == LifeState.Alive )
					anyWormAlive = true;
			}

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

		public float GetHealth()
		{
			float health = 0;

			foreach ( var worm in Worms )
				health += worm.Health;

			return health;
		}

		public void OnWormKilled( Worm worm )
		{
			Worms.Remove( worm );

			// Replace this with a dead worm later.
			worm.Delete();

			// Check how many worms this player has left, if it's 0 then remove this player from the StateHandler list.
			if ( Worms.Count <= 0 )
				StateHandler.Instance?.RemovePlayer( this );
		}

		[ClientRpc]
		public void UpdateCameraTarget( Entity target )
		{
			(Camera as Pawn.Camera).SetLookTarget( target );
		}
	}
}
