using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace TerryForm.Weapons
{
	public partial class Projectile : ModelEntity
	{
		private List<ArcSegment> Segments { get; set; }
		private float MoveSpeed { get; set; }
		private Vector3 LastPos { get; set; }
		public bool IsCompleted { get; set; }

		public override void Spawn()
		{
			//SetupPhysicsFromModel( PhysicsMotionType.Dynamic );

			base.Spawn();
		}

		public Projectile WithModel( string modelPath )
		{
			SetModel( modelPath );
			return this;
		}

		public Projectile MoveAlongTrace( List<ArcSegment> points, float speed = 20 )
		{
			Segments = points;

			// Set the initial position
			Position = Segments[0].StartPos;
			LastPos = Position;

			return this;
		}

		public override void Simulate( Client cl )
		{
			// This might be shite
			if ( Segments is null ) return;
			if ( IsCompleted == true ) return;

			DebugOverlay.Sphere( Segments[1].StartPos, 2f, Color.Red );

			if ( Position.IsNearlyEqual( Segments[1].StartPos, 1f ) )
			{
				Segments.RemoveAt( 0 );

				if ( Segments.Count == 1 )
				{
					Log.Info( "KABOOM" );
					IsCompleted = true;

					return;
				}

				Log.Info( "Arrived at destination" );
			}
			else
			{
				Position = Vector3.Lerp( LastPos, Segments[0].EndPos, 60f * Time.Delta );
			}

			LastPos = Position;
			DebugOverlay.Sphere( Position, 2f, Color.Green );
		}
	}
}
