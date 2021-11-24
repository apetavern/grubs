using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public partial class Player : Entity
	{
		public List<Worm> Worms { get; set; }
		public Worm ActiveWorm { get; set; }


		public Clothing.Container clothes { get; set; }

		public Player(Client cl)
		{
			Worms = new();


			clothes = new();

			clothes.LoadFromClient( cl );

			List<Clothing> yeetclothes = new List<Clothing>();

			for ( int i = 0; i < clothes.Clothing.Count; i++ )
			{
				if ( clothes.Clothing[i].Category == Clothing.ClothingCategory.Bottoms || clothes.Clothing[i].Category == Clothing.ClothingCategory.Footwear || clothes.Clothing[i].Category == Clothing.ClothingCategory.Tops )
				{
					yeetclothes.Add( clothes.Clothing[i] );
				}
			}

			foreach ( var item in yeetclothes )
			{
				clothes.Clothing.Remove( item );
			}

			

			for ( int i = 0; i < GameConfig.WormCount; i++ )
			{
				var worm = new Worm();
				worm.Respawn();
				clothes.DressEntity( worm );
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
