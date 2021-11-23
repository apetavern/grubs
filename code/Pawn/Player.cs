using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public partial class Player : Entity
	{
		public List<Worm> Worms { get; set; }
		public Worm ActiveWorm { get; set; }

		public Player()
		{
			Worms = new();

			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Respawn();
				Worms.Add( worm );
			}

			ActiveWorm = Worms[0];
		}

		public void OnTurnStart()
		{
			ActiveWorm = Worms[0];
		}

		public void OnTurnEnd()
		{
			if ( ActiveWorm.Health < 0 )
			{
				ActiveWorm.OnKilled();
			}

			RotateWorms();
		}

		public void RotateWorms()
		{
			var current = Worms[0];
			Worms.RemoveAt( 0 );
			Worms.Add( current );

			ActiveWorm = Worms[0];
		}
	}
}
