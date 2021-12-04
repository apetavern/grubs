using Sandbox;
using System.Collections.Generic;
using System.Linq;
using TerryForm.Pawn;
using TerryForm.Utils;

namespace TerryForm.Weapons
{
	public partial class Projectile : ModelEntity, IAwaitResolution
	{
		public bool IsResolved { get; set; }
		private TimeSince TimeSinceSegmentStarted { get; set; }
		private float Speed { get; set; }
		private List<ArcSegment> Segments { get; set; }
		private Particles TrailParticles { get; set; }

		public Projectile WithModel( string modelPath )
		{
			SetModel( modelPath );
			return this;
		}

		public Projectile MoveAlongTrace( List<ArcSegment> points, float speed = 1000 )
		{
			Segments = points;

			// Set the initial position
			Position = Segments[0].StartPos;
			Speed = 1 / speed;

			if ( IsServer )
				CreateTrailEffects();

			//DebugOverlay.Sphere( Segments[Segments.Count - 1].EndPos, 2, Color.Red, false, 30 );

			return this;
		}

		public void DrawSegments()
		{
			foreach ( var segment in Segments )
			{
				DebugOverlay.Line( segment.StartPos, segment.EndPos );
			}
		}

		[Event.Tick]
		public void Tick()
		{
			// This might be shite
			if ( Segments is null || !Segments.Any() )
				return;

			if ( IsResolved == true )
				return;

			DrawSegments();

			if ( Position.IsNearlyEqual( Segments[0].EndPos ) )
			{
				Segments.RemoveAt( 0 );

				if ( Segments.Count < 1 )
				{
					OnCollision();

					return;
				}

				TimeSinceSegmentStarted = 0;
			}
			else
			{
				Rotation = Rotation.LookAt( Segments[0].EndPos - Segments[0].StartPos );
				Position = Vector3.Lerp( Segments[0].StartPos, Segments[0].EndPos, Time.Delta / Speed );
			}
		}

		public void OnCollision()
		{
			IsResolved = true;

			if ( !IsServer )
				return;

			DoBlastWithRadius();
			OnCollisionEffects();

			Delete();
		}

		public void DoBlastWithRadius( float radius = 100f )
		{
			var effectedEntities = Physics.GetEntitiesInSphere( Position, radius ).OfType<Worm>();

			foreach ( var entity in effectedEntities )
				entity.TakeDamage( new DamageInfo() { Position = Position, Flags = DamageFlags.Blast, Damage = 0 } );
		}

		[ClientRpc]
		public void CreateTrailEffects()
		{
			TrailParticles = Particles.Create( "particles/smoke_trail.vpcf" );
			TrailParticles.SetEntityAttachment( 0, this, "trail" );
		}

		[ClientRpc]
		public void OnCollisionEffects()
		{
			Particles.Create( "particles/explosion/barrel_explosion/explosion_fire_ring.vpcf", Position );

			TrailParticles?.Destroy();
		}
	}
}
