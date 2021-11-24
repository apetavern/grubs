using Sandbox;
using System.Collections.Generic;
using TerryForm.Utils;

namespace TerryForm.Pawn
{
	public partial class Player : Entity
	{
		public List<Worm> Worms { get; set; }
		[Net] public Worm ActiveWorm { get; set; }
		public Client ClientOwner { get; set; }
		[Net] public long ClientId { get; set; }


		public Clothing.Container clothes { get; set; }

		public Player( Client cl )
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
