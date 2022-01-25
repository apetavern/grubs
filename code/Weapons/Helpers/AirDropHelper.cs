using System.Linq;
using Sandbox;
using Grubs.Pawn;
using Grubs.Terrain;

namespace Grubs.Weapons.Helpers
{
	public partial class AirDropHelper : Entity
	{
		static AirDropHelper Instance { get; set; }

		public static void SummonDrop<T>( Entity droppedEntity, AirDropTravelDirection travelDirection = AirDropTravelDirection.Right )
		{
			if ( Instance is null )
				Instance = new();

			Log.Info( $"Summon aircraft dropping {droppedEntity}" );
		}
	}

	public enum AirDropTravelDirection
	{
		Left,
		Right
	}
}
