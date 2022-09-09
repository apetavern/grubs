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
	/// Penetrate terrain? (Leaves trail in the terrain until end of trace)
	/// </summary>
	protected virtual string FireSound => AssetDefinition.firesound;

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
		Host.AssertServer();

		if ( TraceDelay == 0 )
		{

			GrubsGame.Current.CurrentGamemode.UseTurn();

			PlaySound( FireSound );

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
			await ShootGrubsAsync();
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
				var system = Particles.Create( "particles/guntrace.vpcf" );
				
				Vector3 OffsetSpread = Vector3.Random * TraceSpread;

				if ( debugtraceweap )
				{
					DebugOverlay.Line( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, 2.5f );
				}

				TraceResult result = Trace.Ray( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).Ignore( Parent ).UseHitboxes().Run();

				if ( PenetrateTargets )
				{
					for ( int g = 0; g < All.OfType<Grub>().Count(); g++ )
					{
						if ( result.Entity is Grub grub )
						{
							if ( !hitgrubs.Contains( grub ) )
								hitgrubs.Add( grub );
							result = Trace.Ray( result.EndPosition.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Ignore( grub ).Ignore( Parent ).Run();

							if ( result.Hit && result.Entity is not Grub )
							{
								ExplosionHelper.Explode( result.EndPosition, Parent as Grub, ExplosionRadius, 0 );
							}

							if ( debugtraceweap )
							{
								DebugOverlay.Sphere( result.EndPosition, 10f, Color.Red, 2.5f );
							}
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
					ExplosionHelper.DrawLine( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, ExplosionRadius );

				}
				else if ( result.Hit )
				{
					ExplosionHelper.Explode( result.EndPosition, Parent as Grub, ExplosionRadius, 0 );
				}
				system?.SetPosition( 0, muzzle.Position );
				system?.SetPosition( 1, result.EndPosition );
			}

		}

		return hitgrubs;
	}

	public async Task ShootGrubsAsync()
	{
		var hitgrubs = new List<Grub>();

		if ( GetAttachment( "muzzle" ).HasValue )
		{
			Transform muzzle = GetAttachment( "muzzle" ).Value;
			for ( int i = 0; i < TraceCount; i++ )
			{
				var system = Particles.Create( "particles/guntrace.vpcf" );
				muzzle = GetAttachment( "muzzle" ).Value;
				Vector3 OffsetSpread = Vector3.Random * TraceSpread;

				if ( debugtraceweap )
				{
					DebugOverlay.Line( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread, 2.5f );
				}

				TraceResult result = Trace.Ray( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).Ignore(Parent).UseHitboxes().Run();

				if ( PenetrateTargets )
				{
					for ( int g = 0; g < All.OfType<Grub>().Count(); g++ )
					{
						if ( result.Entity is Grub grub )
						{
							if ( !hitgrubs.Contains( grub ) )
								hitgrubs.Add( grub );
							result = Trace.Ray( result.EndPosition.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + OffsetSpread ).UseHitboxes().Ignore( grub ).Ignore( Parent ).Run();

							if ( debugtraceweap )
							{
								DebugOverlay.Sphere( result.EndPosition, 10f, Color.Red, 2.5f );
							}
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
				else if(result.Hit)
				{
					ExplosionHelper.Explode( result.EndPosition, Parent as Grub, ExplosionRadius, 0 );
				}

				foreach ( var grub in hitgrubs )
				{
					HitGrub( grub );

					GrubsCamera.SetTarget( grub );
				}

				PlaySound( FireSound );

				system?.SetPosition( 0, muzzle.Position );
				system?.SetPosition( 1, result.EndPosition );

				await Task.DelaySeconds( TraceDelay );
			}
			GrubsGame.Current.CurrentGamemode.UseTurn();
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
