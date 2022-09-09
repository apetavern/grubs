using System.Net;
using System.Threading.Tasks;
using Grubs.Player;
using Grubs.Utils;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon capable of firing projectiles.
/// </summary>
public class HitscanWeapon : GrubWeapon
{
	/// <summary>
	/// The multiplier for how far it should knock back a grub when hit
	/// </summary>
	protected virtual float HitForce => AssetDefinition.HitForce;

	/// <summary>
	/// How many bullets should be fired when shot
	/// </summary>
	protected virtual int TraceCount => AssetDefinition.TraceCount;

	/// <summary>
	/// Spread of the trace, randomizes the trace a bit
	/// </summary>
	protected virtual float TraceSpread => AssetDefinition.TraceSpread;

	/// <summary>
	/// Spread of the trace, randomizes the trace a bit
	/// </summary>
	protected virtual float TraceDelay => AssetDefinition.TraceDelay;

	/// <summary>
	/// Spread of the trace, randomizes the trace a bit
	/// </summary>
	protected virtual float ExplosionRadius => AssetDefinition.ExplosionRadius;

	/// <summary>
	/// Penetrate targets?
	/// </summary>
	protected virtual bool PenetrateTargets => AssetDefinition.PenetrateTargets;

	/// <summary>
	/// Penetrate terrain? (Leaves trail in the terrain until end of trace)
	/// </summary>
	protected virtual bool PenetrateTerrain => AssetDefinition.PenetrateTerrain;

	/// <summary>
	/// The amount of damage being hit by the weapon will do.
	/// <remarks>This may be unused if <see cref="HitGrub"/> is overridden.</remarks>
	/// </summary>
	// TODO: Damage falloff based on range?
	protected virtual float Damage => AssetDefinition.Damage;

	/// <summary>
	/// The damage flags to attach to the damage info.
	/// <remarks>This may be unused if <see cref="HitGrub"/> is overridden.</remarks>
	/// </summary>
	protected virtual DamageFlags DamageFlags => AssetDefinition.DamageFlags;


	protected new HitscanWeaponAsset AssetDefinition => (base.AssetDefinition as HitscanWeaponAsset)!;

	public HitscanWeapon()
	{
	}

	public HitscanWeapon( HitscanWeaponAsset assetDefinition ) : base( assetDefinition )
	{
	}

	protected override async Task OnFire()
	{
		await base.OnFire();

		if ( TraceDelay == 0 )
		{
			var grubsHit = GetGrubsInShot();

			/*if ( !PenetrateTargets )
			{
				Grub closestGrub = null!;
				var closestGrubDistance = float.MaxValue;

				foreach ( var grub in grubsHit )
				{
					var distance = Position.Distance( grub.Position );
					if ( distance > closestGrubDistance )
						continue;

					closestGrub = grub;
					closestGrubDistance = distance;
				}

				grubsHit = new List<Grub> { closestGrub };
			}*/

			foreach ( var grub in grubsHit )
			{
				HitGrub( grub );

				GrubsCamera.SetTarget( grub );
			}
		}
		else
		{
			ShootGrubsAsync();
		}
	}

	bool debugtraceweap = false;

	public List<Grub> GetGrubsInShot()
	{
		var hitgrubs = new List<Grub>();

		if ( GetAttachment( "muzzle" ).HasValue )
		{
			Transform muzzle = GetAttachment( "muzzle" ).Value;
			for ( int i = 0; i < TraceCount; i++ )
			{
				Vector3 OffsetSpread = Vector3.Random * TraceSpread;

				if( debugtraceweap )
				{
					DebugOverlay.Line( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, 2.5f );
				}

				TraceResult result = Trace.Ray( muzzle.Position.WithY(0), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Run();

				if ( PenetrateTargets )
				{
					while ( result.Entity is Grub grub )
					{
						if ( !hitgrubs.Contains( grub ) )
							hitgrubs.Add( grub );
						result = Trace.Ray( result.EndPosition.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Ignore( grub ).Run();


						if ( debugtraceweap )
						{
							DebugOverlay.Sphere( result.EndPosition, 10f, Color.Red, 2.5f );
						}
					}
				}
				else if ( result.Entity is Grub grub )
				{
					if ( !hitgrubs.Contains( grub ) )
						hitgrubs.Add( grub );
				}

				if ( PenetrateTerrain )
				{
					GrubsGame.Current.TerrainMap.DestructLine( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, ExplosionRadius );
					GrubsGame.LineClient( To.Everyone, muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, ExplosionRadius );

					GrubsGame.Current.RegenerateMap();
				}
				else
				{
					ExplosionHelper.Explode( result.EndPosition, Parent as Grub, ExplosionRadius, 0 );
				}
			}

		}

		return hitgrubs;
	}

	public async void ShootGrubsAsync()
	{
		var hitgrubs = new List<Grub>();

		if ( GetAttachment( "muzzle" ).HasValue )
		{
			Transform muzzle = GetAttachment( "muzzle" ).Value;
			for ( int i = 0; i < TraceCount; i++ )
			{
				Vector3 OffsetSpread = Vector3.Random * TraceSpread;

				if ( debugtraceweap )
				{
					DebugOverlay.Line( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, 2.5f );
				}

				TraceResult result = Trace.Ray( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Run();

				if ( PenetrateTargets )
				{
					while ( result.Entity is Grub grub )
					{
						if ( !hitgrubs.Contains( grub ) )
							hitgrubs.Add( grub );
						result = Trace.Ray( result.EndPosition.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Ignore( grub ).Run();


						if ( debugtraceweap )
						{
							DebugOverlay.Sphere( result.EndPosition, 10f, Color.Red, 2.5f );
						}
					}
				}
				else if ( result.Entity is Grub grub )
				{
					if ( !hitgrubs.Contains( grub ) )
						hitgrubs.Add( grub );
				}

				if ( PenetrateTerrain )
				{
					GrubsGame.Current.TerrainMap.DestructLine( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, ExplosionRadius );
					GrubsGame.LineClient( To.Everyone, muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, ExplosionRadius );

					GrubsGame.Current.RegenerateMap();
				}
				else
				{
					ExplosionHelper.Explode( result.EndPosition, Parent as Grub, ExplosionRadius, 0 );
				}

				foreach ( var grub in hitgrubs )
				{
					HitGrub( grub );

					GrubsCamera.SetTarget( grub );
				}

				await Task.DelaySeconds( TraceDelay );
			}

		}
	}

	/// <summary>
	/// Called for each grub that has been hit by the swing.
	/// </summary>
	/// <param name="grub">The grub that was hit.</param>
	protected virtual void HitGrub( Grub grub )
	{
		var dir = (grub.Position - Position).Normal;
		grub.ApplyAbsoluteImpulse( dir * HitForce );

		grub.TakeDamage( new DamageInfo
		{
			Attacker = Parent,
			Damage = Damage,
			Flags = DamageFlags,
			Position = Position
		} );
	}

}
