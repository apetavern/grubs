using System.Linq;
using Sandbox;
using Grubs.Pawn;
using Grubs.Terrain;
using System.ComponentModel;

namespace Grubs.Weapons.Helpers
{
	public partial class ExplosionHelper : Entity
	{
		static ExplosionHelper Instance { get; set; }

		public static void DoBlastWithRadius( Vector3 origin, float radius = 100f )
		{
			Host.AssertServer();

			if ( !Host.IsServer )
				return;

			if ( Instance is null )
				Instance = new();

			var effectedEntities = Physics.GetEntitiesInSphere( origin, radius ).OfType<Worm>();

			Terrain.Terrain.Update( new Circle( origin, radius, SDF.MergeType.Subtract ) );

			foreach ( var entity in effectedEntities )
				entity.TakeDamage( new DamageInfo() { Position = origin, Flags = DamageFlags.Blast, Damage = 0 } );

			DoExplosionEffectsAt( origin, radius );
		}

		[ClientRpc]
		public static void DoExplosionEffectsAt( Vector3 position, float radius )
		{
			var explosion = Particles.Create( "particles/explosion/grubs_explosion_base.vpcf", position );
			explosion.SetPosition( 1, new Vector3( radius, 0, 0 ) );
		}
	}
}
