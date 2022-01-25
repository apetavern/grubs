using System.Linq;
using Sandbox;
using Grubs.Pawn;
using Grubs.Terrain;
using Grubs.Utils;

namespace Grubs.Weapons.Helpers
{
	public partial class AirDropHelper : Entity, IAwaitResolution
	{
		static AirDropHelper Instance { get; set; }
		public AnimEntity Plane { get; set; }
		public bool IsResolved { get; set; }

		public static void SummonDrop( Entity droppedEntity, AirDropTravelDirection travelDirection = AirDropTravelDirection.Right )
		{
			if ( Instance is null )
				Instance = new();

			Instance.Plane = new AnimEntity( "models/weapons/airstrikes/plane.vmdl" );

			Log.Info( $"Summon aircraft dropping {droppedEntity}" );

			// Move this later once the plane movement stuff is in.
			Instance.IsResolved = true;
		}

		[Event.Tick.Server]
		public void Move()
		{
			// Movement
		}
	}

	public enum AirDropTravelDirection
	{
		Left,
		Right
	}
}
