using Grubs.Player;

namespace Grubs.Utils;

public class ExplosionHelper : Entity
{
	private static ExplosionHelper Instance { get; set; }

	public static void DoBlastWithRadius( Vector3 origin, float radius = 100f )
	{
		Host.AssertServer();

		if ( !Host.IsServer )
			return;

		if ( Instance is null || !Instance.IsValid )
			Instance = new ExplosionHelper();

		var effectedEntities = FindInSphere( origin, radius ).OfType<Worm>();

		foreach ( var entity in effectedEntities )
			entity.TakeDamage( new DamageInfo {Position = origin, Flags = DamageFlags.Blast, Damage = 0} );
		
		DebugOverlay.Sphere( origin, radius, Color.Red, 1 );
	}
}
