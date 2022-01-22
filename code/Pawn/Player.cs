using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.Utils;
using Grubs.Weapons;
using Grubs.States;

namespace Grubs.Pawn
{
	public partial class Player : Sandbox.Player
	{
		[Net] public IList<Worm> Worms { get; set; } = new List<Worm>();
		[Net] public Worm ActiveWorm { get; set; }
		[Net] public PlayerInventory PlayerInventory { get; set; }

		public Player()
		{
			PlayerInventory = new PlayerInventory() { Owner = this };
		}

		public void CreateWorms( Client cl )
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
				PlayerInventory.Add( Library.Create<Weapon>( weapon ) );
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

			// Temporarily allow worms to noclip.
			if ( Input.Released( InputButton.Reload ) )
				Game.Current?.DoPlayerNoclip( cl );
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
			// Replace this with a dead worm later.
			worm.Delete();

			Worms.Remove( worm );

			// Check how many worms this player has left, if it's 0 then remove this player from the StateHandler list.
			if ( Worms.Count <= 0 )
				StateHandler.Instance?.RemovePlayer( this );
		}

		[ClientRpc]
		public void UpdateCameraTarget( Entity target )
		{
			(Camera as Camera).SetLookTarget( target );
		}
	}
}
