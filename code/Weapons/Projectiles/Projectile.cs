using Sandbox;
using System.Collections.Generic;
using System.Linq;
using Grubs.Pawn;
using Grubs.Utils;
using Grubs.Weapons.Helpers;

namespace Grubs.Weapons
{
	public enum ProjectileCollisionReaction
	{
		Explosive,
		Incendiary
	}

	public partial class Projectile : ModelEntity, IAwaitResolution
	{
		public bool IsResolved { get; set; }
		private TimeSince TimeSinceSegmentStarted { get; set; }
		private float Speed { get; set; }
		private List<ArcSegment> Segments { get; set; }
		private Particles TrailParticles { get; set; }
		private float CollisionExplosionDelaySeconds { get; set; }
		private ProjectileCollisionReaction CollisionReaction { get; set; }

		public Projectile WithModel( string modelPath )
		{
			SetModel( modelPath );
			return this;
		}

		/// <summary>
		/// What happens when the projectile "collides".
		/// </summary>
		/// <param name="reaction"></param>
		/// <returns></returns>
		public Projectile SetCollisionReaction( ProjectileCollisionReaction reaction )
		{
			CollisionReaction = reaction;
			return this;
		}

		/// <summary>
		/// How many seconds after collision should this explode.
		/// </summary>
		/// <param name="secondsDelay"></param>
		/// <returns></returns>
		public Projectile WithCollisionExplosionDelay( float secondsDelay )
		{
			CollisionExplosionDelaySeconds = secondsDelay;
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
			// Don't move if the projectile is parented.
			if ( Parent.IsValid() )
				return;

			// This might be shite
			if ( Segments is null || !Segments.Any() )
				return;

			if ( IsResolved == true )
				return;

			DrawSegments();

			if ( Position.IsNearlyEqual( Segments[0].EndPos, 2.5f ) )
			{
				if ( Segments.Count > 1 )
					Segments.RemoveAt( 0 );
				else
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

			if ( CollisionExplosionDelaySeconds > 0 )
			{
				ExplodeAfterSeconds( CollisionExplosionDelaySeconds );
				return;
			}

			Explode();
		}

		/// <summary>
		/// How many seconds after firing should this explode.
		/// </summary>
		/// <param name="seconds"></param>
		public async void ExplodeAfterSeconds( float seconds )
		{
			await GameTask.DelaySeconds( seconds );
			Explode();
		}

		private void Explode()
		{
			switch ( CollisionReaction )
			{
				case ProjectileCollisionReaction.Explosive:
					ExplosionHelper.DoBlastWithRadius( Position );
					break;
				case ProjectileCollisionReaction.Incendiary:
					FireHelper.StartFiresAt( Position, (Segments[Segments.Count - 1].EndPos - Segments[Segments.Count - 1].StartPos), 1 );
					Log.Info( Segments.Count );
					break;
			}

			OnDelete();
			Delete();
		}

		[ClientRpc]
		public void CreateTrailEffects()
		{
			TrailParticles = Particles.Create( "particles/smoke_trail.vpcf" );
			TrailParticles.SetEntityAttachment( 0, this, "trail" );
		}

		[ClientRpc]
		public void OnDelete()
		{
			TrailParticles?.Destroy();
		}
	}
}
