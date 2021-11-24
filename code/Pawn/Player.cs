using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public partial class Player : Entity
	{
		public List<Worm> Worms { get; set; }
		public Worm ActiveWorm { get; set; }
		public Client ClientOwner { get; set; }

		public Player()
		{
			Worms = new();

			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Respawn();
				Worms.Add( worm );
			}
		}

		public void InitializeFromClient( Client cl )
		{
			ClientOwner = cl;

			PickNextWorm();
		}

		public void OnTurnStart()
		{
			PickNextWorm();
		}

		public void OnTurnEnd()
		{
			if ( ActiveWorm.Health < 0 )
				ActiveWorm.OnKilled();
		}

		private void RotateWorms()
		{
			var current = Worms[0];
			Worms.RemoveAt( 0 );
			Worms.Add( current );
		}

		public void PickNextWorm()
		{
			Log.Info( $"Picking new worm for player {ClientOwner.Name}" );

			RotateWorms();
			ActiveWorm = Worms[0];

			ClientOwner.Pawn = ActiveWorm;
		}
	}
}
