using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;
using TerryForm.Weapons;
using System.Linq;

namespace TerryForm.Pawn
{
	public partial class Player : Sandbox.Player
	{
		public List<Worm> Worms { get; set; } = new();
		[Net] public Worm ActiveWorm { get; set; }
		[Net] public bool IsAlive { get; set; }

		public Player() { }

		public Player( Client cl ) : this()
		{
			IsAlive = true;

			Inventory = new Inventory( this );
			foreach ( var weapon in Library.GetAll<Weapon>().Where( weapon => !weapon.IsAbstract ) )
			{

			}

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

			// Simulate all worms, this might seem odd but without this the worm never grounds because it's controller isn't simulated.
			Worms.ForEach( worm => worm.Simulate( cl ) );
		}

		protected void InitializeFromClient( Client cl )
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
