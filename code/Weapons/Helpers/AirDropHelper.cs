using System;
using Sandbox;
using Grubs.Utils;

namespace Grubs.Weapons.Helpers
{
	public partial class AirDropHelper : AnimEntity, IAwaitResolution
	{
		public static AirDropHelper Instance { get; set; }
		private static Vector3 Origin => new Vector3( -1000, 0, 400 );
		private static Vector3 Destination => new Vector3( 1000, 0, 400 );
		private static float MovementSpeed = 450f;
		public bool IsResolved { get; set; }
		private Vector3 TargetPosition { get; set; }
		private Entity EntityToDrop { get; set; }

		public static void SummonDropWithTarget( Entity droppedEntity, Vector3 target )
		{
			Host.AssertServer();

			if ( !Host.IsServer )
				return;

			if ( Instance is null || !Instance.IsValid )
				Instance = new();

			Instance.TargetPosition = target;
			SummonDrop( droppedEntity );
		}

		private static void SummonDrop( Entity droppedEntity )
		{
			Instance.SetModel( "models/weapons/airstrikes/plane.vmdl" );
			Instance.Position = Origin;

			droppedEntity.Parent = Instance;
			Instance.EntityToDrop = droppedEntity;
		}

		private void Drop()
		{

		}

		[Event.Tick.Server]
		public void Move()
		{
			Position += Vector3.Forward * MovementSpeed * Time.Delta;

			if ( Math.Abs( Position.x - Destination.x ) < 50 )
			{
				IsResolved = true;

				EntityToDrop?.Delete();
				Delete();
			}

			if ( TargetPosition != Vector3.Zero )
				DebugOverlay.Sphere( TargetPosition, 30f, Color.Red );
		}
	}
}
