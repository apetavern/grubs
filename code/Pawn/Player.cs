using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public partial class Player : Entity
	{
		public List<Worm> Worms { get; set; } = new();
		[Net] public Worm ActiveWorm { get; set; }
		public Client ClientOwner { get; set; }
		[Net] public long ClientId { get; set; }

		public Player( Client cl )
		{
			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Respawn();
				worm.DressFromClient( cl );

				Worms.Add( worm );
			}
		}

		public void InitializeFromClient( Client cl )
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

			ClientOwner.Pawn = ActiveWorm;
		}
	}
}
