using Grubs.Common;
using Grubs.Player;

namespace Grubs.Helpers;

[Title( "Grubs - Explosion Helper" ), Category( "World" )]
public sealed class ExplosionHelperComponent : Component
{
	public static ExplosionHelperComponent Instance { get; set; } = new();

	public ExplosionHelperComponent()
	{
		Instance = this;
	}

	public void Explode( Component source, Vector3 position, float radius, float damage )
	{
		var gos = Scene.FindInPhysics( new Sphere( position, radius ) );
		foreach ( var go in gos )
		{
			if ( !go.Components.TryGet( out HealthComponent health, FindMode.EverythingInSelfAndAncestors ) )
				continue;

			var dist = Vector3.DistanceBetween( position, go.Transform.Position );
			var distFactor = 1.0f - MathF.Pow( dist / radius, 2 ).Clamp( 0, 1 );

			if ( go.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
				HandleGrubExplosion( grub, position );

			Log.Info( $"{go.Name} is taking {damage * distFactor} damage from {source.GameObject.Name}." );
			health.TakeDamage( damage * distFactor );
		}
	}

	private void HandleGrubExplosion( Grub grub, Vector3 position )
	{
		var dir = (grub.Transform.Position - position).Normal;
		dir = dir.WithY( 0f );

		grub.CharacterController.Punch( (dir + Vector3.Up) * 256f );
		grub.CharacterController.ReleaseFromGround();
	}
}
