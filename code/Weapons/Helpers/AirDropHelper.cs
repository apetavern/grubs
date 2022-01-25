using System;
using Sandbox;
using Grubs.Utils;

namespace Grubs.Weapons.Helpers
{
	public partial class AirDropHelper : AnimEntity, IAwaitResolution
	{
		static AirDropHelper Instance { get; set; }
		private Entity EntityToDrop { get; set; }
		private static float OriginX = -1000;
		private static float DestinationX = 1000;
		private static float MovementSpeed = 450f;
		public bool IsResolved { get; set; }


		public static void DoDrop( Entity droppedEntity )
		{
			if ( Instance is null )
				Instance = new();

			Instance.SetModel( "models/weapons/airstrikes/plane.vmdl" );
			Instance.Position = Vector3.Forward * OriginX + Vector3.Up * 400;

			Instance.EntityToDrop = droppedEntity;
		}

		[Event.Tick.Server]
		public void Move()
		{
			// Movement
			Position += Vector3.Forward * MovementSpeed * Time.Delta;

			if ( Math.Abs( Position.x - DestinationX ) < 50 )
			{
				IsResolved = true;

				EntityToDrop?.Delete();
				Delete();
			}
		}
	}
}
