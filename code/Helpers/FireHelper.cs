using Grubs.Pawn;
using Grubs.Terrain;
using Sandbox.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Grubs.Helpers;

[Title( "Grubs - Fire Helper" ), Category( "World" )]
public partial class FireHelper : Component
{
	public static FireHelper Local { get; set; }

	[Property] private float fireLifetime { get; set; } = 3f;

	[Property] public float minParticleSpeed { get; set; } = 100f;

	public FireHelper()
	{
		Local = this;
	}

	[Sync] public List<Vector3> fireParticlePositions { get; set; } = new List<Vector3>();
	[Sync] public List<Vector3> fireParticleVelocities { get; set; } = new List<Vector3>();
	[Sync] public List<float> fireParticleLifetimes { get; set; } = new List<float>();

	[Property] private List<GameObject> fireObjects { get; set; } = new List<GameObject>();

	[Property] private GameObject FireObjectPrefab { get; set; }

	protected override void OnUpdate()
	{
		base.OnUpdate();

		if ( fireObjects.Count != fireParticlePositions.Count )
		{
			if ( fireObjects.Count > fireParticlePositions.Count )
			{
				fireObjects[0].Destroy();
				fireObjects.RemoveAt( 0 );
			}
			else
			{
				fireObjects.Add( FireObjectPrefab.Clone() );
			}
		}

		for ( int i = 0; i < fireObjects.Count; i++ )
		{
			fireObjects[i].Transform.Position = Vector3.Lerp( fireObjects[i].Transform.Position, fireParticlePositions[i], Time.Delta * Vector3.DistanceBetween( fireObjects[i].Transform.Position, fireParticlePositions[i] ) );
		}

		if ( IsProxy ) return;

		for ( int i = 0; i < fireParticlePositions.Count; i++ )
		{

			if ( fireParticleLifetimes[i] > fireLifetime )
			{
				fireParticlePositions.RemoveAt( i );
				fireParticleVelocities.RemoveAt( i );
				fireParticleLifetimes.RemoveAt( i );
				fireObjects[i].Destroy();
				fireObjects.RemoveAt( i );
				break;
			}

			ParticleTick( i );
		}
	}

	public void ParticleTick( int particle )
	{
		fireParticleLifetimes[particle] += Time.Delta;

		fireParticlePositions[particle] += fireParticleVelocities[particle] * Time.Delta * 5f;

		if ( fireParticleVelocities[particle].Length > Local.minParticleSpeed )
		{
			fireParticleVelocities[particle] *= 0.95f;
		}

		var gos = Scene.FindInPhysics( new Sphere( fireParticlePositions[particle], 10f ) );
		foreach ( var go in gos )
		{
			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubExplosion( grub, fireParticlePositions[particle] );
		}

		fireParticleVelocities[particle] += Vector3.Down;

		var tr = Scene.Trace.Ray( fireParticlePositions[particle], fireParticlePositions[particle] + fireParticleVelocities[particle].Normal * 5f ).Run();

		if ( tr.Hit )
		{
			fireParticleLifetimes[particle] += Time.Delta;

			fireParticleVelocities[particle] = Vector3.Reflect( fireParticleVelocities[particle], tr.Normal );

			fireParticleVelocities[particle] *= 0.1f;

			fireParticleVelocities[particle] += new Vector3( Game.Random.Float( -10f, 10f ), 0, 0 );

			var startPos = fireParticlePositions[particle];

			var TorchSize = 6f;

			var endPos = fireParticlePositions[particle] + fireParticleVelocities[particle].Normal * TorchSize;

			using ( Rpc.FilterInclude( c => c.IsHost ) )
			{
				GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ), TorchSize, 1 );
				GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ),
					TorchSize + 8f );
			}
		}
	}

	private void HandleGrubExplosion( Grub grub, Vector3 position )
	{
		var dir = (grub.Transform.Position - position).Normal;
		dir = dir.WithY( 0f );

		grub.CharacterController.Punch( dir * 16f );
		grub.CharacterController.ReleaseFromGround();
	}

	[Broadcast]
	public void CreateFire( FireParticle particle )
	{
		fireParticlePositions.Add( particle.Position );
		fireParticleVelocities.Add( particle.Velocity );
		fireParticleLifetimes.Add( 0f );
	}
}

public struct FireParticle
{
	public Vector3 Position { get; set; }
	public Vector3 Velocity { get; set; }
	public TimeSince TimeSinceCreated { get; set; }
}
