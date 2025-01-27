using Grubs.Common;
using Grubs.Pawn;
using Grubs.Systems.Pawn.Grubs;
using Grubs.Terrain;

namespace Grubs.Helpers;

[Title( "Grubs - Explosion Helper" ), Category( "World" )]
public partial class ExplosionHelper : Component
{
	public static ExplosionHelper Instance { get; set; } = new();

	public ExplosionHelper()
	{
		Instance = this;
	}

	public void Explode( Component source, Vector3 position, float radius, float damage, Guid attackerGuid, string attackerName, float force = 128f )
	{
		var gos = Scene.FindInPhysics( new Sphere( position, radius ) );
		foreach ( var go in gos )
		{
			if ( source?.GameObject == go )
				continue;

			if ( !go.Components.TryGet( out Health health, FindMode.EverythingInSelfAndAncestors ) )
				continue;

			var dist = Vector3.DistanceBetween( position, go.WorldPosition );
			var distFactor = 1.0f - MathF.Pow( dist / radius, 2 ).Clamp( 0, 1 );

			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubExplosion( grub, position, force );

			if ( go.Components.TryGet( out Rigidbody body, FindMode.EverythingInSelf ) )
				HandlePhysicsExplosion( body, position, force );

			health.TakeDamage( GrubsDamageInfo.FromExplosion( damage * distFactor, attackerGuid, attackerName, position ) );
		}

		LastPosition = position;
		LastRadius = radius;

		using ( Rpc.FilterInclude( c => c.IsHost ) )
		{
			GrubsTerrain.Instance.SubtractCircle( new Vector2( position.x, position.z ), radius / 2f, 1 );
			GrubsTerrain.Instance.ScorchCircle( new Vector2( position.x, position.z ), radius / 2f + 8f );
		}
	}

	private void HandleGrubExplosion( Grub grub, Vector3 position, float force )
	{
		var dir = (grub.WorldPosition - position).Normal;
		dir = dir.WithY( 0f );

		grub.CharacterController.Punch( (dir + Vector3.Up * 2f) * force );
		grub.CharacterController.ReleaseFromGround();
		
		GrubFollowCamera.Local?.QueueTarget( grub.GameObject );
	}

	private void HandlePhysicsExplosion( Rigidbody body, Vector3 position, float force )
	{
		var dir = (body.WorldPosition - position).Normal;
		dir = dir.WithY( 0f );

		body.ApplyImpulseAt(
			body.WorldPosition + Vector3.Down * 0.25f,
			dir * force * body.PhysicsBody.Mass );
	}

	private Vector3 LastPosition { get; set; }
	private float LastRadius { get; set; }

	protected override void DrawGizmos()
	{
		base.DrawGizmos();

		Gizmo.Transform = global::Transform.Zero;

		Gizmo.Draw.LineSphere( LastPosition, LastRadius );
	}
}
