using Grubs.Player;
using Grubs.Utils;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon capable of firing projectiles.
/// </summary>
public partial class HitscanWeapon : GrubWeapon
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
	protected virtual string FireSound => AssetDefinition.FireSound;

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

	/// <summary>
	/// The hitscan asset definition this weapon is implementing.
	/// </summary>
	protected new HitscanWeaponAsset AssetDefinition => (base.AssetDefinition as HitscanWeaponAsset)!;

	/// <summary>
	/// The time since the last hitscan was performed.
	/// </summary>
	[Net, Predicted]
	public TimeSince TimeSinceLastHitscan { get; private set; }

	/// <summary>
	/// The amount of traces that have been fired in this burst.
	/// </summary>
	[Net, Predicted]
	public int TracesFired { get; private set; }

	public HitscanWeapon()
	{
	}

	public HitscanWeapon( HitscanWeaponAsset assetDefinition ) : base( assetDefinition )
	{
	}

	public override void Simulate( Client cl )
	{
		base.Simulate( cl );

		if ( IsFiring && TimeSinceLastHitscan >= TraceDelay )
			Shoot();
	}

	protected override bool OnFire()
	{
		if ( TraceDelay == 0 && TraceCount > 1 )
		{
			for ( var i = 0; i < TraceCount; i++ )
				Shoot();
		}
		else
			Shoot();

		return TraceDelay > 0;
	}

	/// <summary>
	/// Fires a single hitscan.
	/// </summary>
	protected virtual void Shoot()
	{
		var attachment = GetAttachment( "muzzle" );
		if ( attachment is null )
		{
			Log.Error( "Weapon does not have a \"muzzle\" attachment" );
			return;
		}

		var muzzle = attachment.Value;
		var system = Particles.Create( "particles/guntrace/guntrace.vpcf" );
		var offsetSpread = Vector3.Random * TraceSpread;

		Particles.Create( "particles/muzzleflash/grubs_muzzleflash.vpcf", muzzle.Position );

		var result = Trace.Ray( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + offsetSpread )
			.Ignore( Parent )
			.UseHitboxes()
			.Run();
		if ( HitscanDebug )
			DebugOverlay.TraceResult( result, 5 );

		if ( IsServer )
		{
			var hitgrubs = new List<Grub>();
			if ( PenetrateTargets )
			{
				for ( var i = 0; i < All.OfType<Grub>().Count(); i++ )
				{
					if ( result.Entity is not Grub grub )
						continue;

					if ( !hitgrubs.Contains( grub ) )
						hitgrubs.Add( grub );
					result = Trace.Ray( result.EndPosition.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + offsetSpread )
						.Ignore( grub )
						.Ignore( Parent )
						.UseHitboxes()
						.Run();

					if ( HitscanDebug )
						DebugOverlay.Sphere( result.EndPosition, 10f, Color.Red, 5 );
				}
			}
			else if ( result.Entity is Grub grub )
			{
				if ( !hitgrubs.Contains( grub ) )
					hitgrubs.Add( grub );
			}

			if ( PenetrateTerrain )
			{
				GrubsGame.Current.TerrainMap.DestructLine( muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + offsetSpread, ExplosionRadius );
				GrubsGame.LineClient( To.Everyone, muzzle.Position.WithY( 0 ), muzzle.Position.WithY( 0 ) + muzzle.Rotation.Forward.WithY( 0 ) * 5000f + offsetSpread, ExplosionRadius );

				GrubsGame.Current.RegenerateMap();
			}
			else if ( result.Hit )
				ExplosionHelper.Explode( result.EndPosition, Holder, ExplosionRadius, 0 );

			foreach ( var grub in hitgrubs )
				HitGrub( grub );
		}

		PlaySound( FireSound );

		system?.SetPosition( 0, muzzle.Position );
		system?.SetPosition( 1, result.EndPosition );

		TimeSinceLastHitscan = 0;
		TracesFired++;

		if ( TracesFired < TraceCount )
			return;

		IsFiring = false;
		TracesFired = 0;
		OnFireFinish();
	}

	/// <summary>
	/// Called for each grub that has been hit by the swing.
	/// </summary>
	/// <param name="grub">The grub that was hit.</param>
	protected virtual void HitGrub( Grub grub )
	{
		Host.AssertServer();

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

	[ConVar.Replicated( "hitscan_debug" )]
	public static bool HitscanDebug { get; set; } = false;
}
