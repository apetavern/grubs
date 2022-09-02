using System.Threading.Tasks;
using Grubs.Player;
using Grubs.States;

namespace Grubs.Weapons.Base;

/// <summary>
/// A weapon capable of smacking a grub.
/// </summary>
public abstract class MeleeWeapon : GrubWeapon
{
	/// <summary>
	/// Where the hit zone starts.
	/// <remarks>This may be unused if <see cref="GetGrubsInSwing"/> is overridden.</remarks>
	/// </summary>
	protected virtual Vector3 HitStart => Parent.Position - HitSize.WithX( 0 ).WithZ( 0 ) / 2;
	/// <summary>
	/// The size of the hit zone.
	/// <remarks>This may be unused if <see cref="GetGrubsInSwing"/> is overridden.</remarks>
	/// </summary>
	protected virtual Vector3 HitSize => Vector3.One;
	/// <summary>
	/// Whether or not the weapon can hit multiple people at once.
	/// </summary>
	protected virtual bool HitMulti => true;
	/// <summary>
	/// The time in seconds to delay before applying hits. This is to simulate the swing animation and hit when the weapon should collide.
	/// </summary>
	protected virtual float HitDelay => 1;
	/// <summary>
	/// The damage flags to attach to the damage info.
	/// <remarks>This may be unused if <see cref="HitGrub"/> is overridden.</remarks>
	/// </summary>
	protected virtual DamageFlags DamageFlags => DamageFlags.Blunt;
	/// <summary>
	/// The amount of damage being hit by the weapon will do.
	/// <remarks>This may be unused if <see cref="HitGrub"/> is overridden.</remarks>
	/// </summary>
	// TODO: Damage falloff based on range?
	protected virtual float Damage => 1;

	protected override async Task OnFire()
	{
		await base.OnFire();
		await GameTask.DelaySeconds( HitDelay );

		var grubsHit = GetGrubsInSwing();
		if ( !HitMulti )
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
		}

		foreach ( var grub in grubsHit )
			HitGrub( grub );
	}

	/// <summary>
	/// Gets all grubs that are in the swing of the melee weapon.
	/// </summary>
	/// <returns>The list of grubs that were hit.</returns>
	protected virtual List<Grub> GetGrubsInSwing()
	{
		var holder = Parent as Grub;
		var mins = Vector3.Min( HitStart, HitStart + (holder!.FacingLeft ? HitSize.WithX( -HitSize.x ) : HitSize) );
		var maxs = Vector3.Max( HitStart, HitStart + (holder.FacingLeft ? HitSize.WithX( -HitSize.x ) : HitSize) );

		var grubsHit = new List<Grub>();
		foreach ( var grub in All.OfType<Grub>() )
		{
			if ( grub == Parent )
				continue;

			var grubPosition = grub.Position;
			if ( grubPosition.x >= mins.x && grubPosition.x <= maxs.x &&
				 grubPosition.y >= mins.y && grubPosition.y <= maxs.y &&
				 grubPosition.z >= mins.z && grubPosition.z <= maxs.z )
				grubsHit.Add( grub );
		}

		if ( MeleeDebug )
			DebugDraw();

		return grubsHit;
	}

	/// <summary>
	/// Called for each grub that has been hit by the swing.
	/// </summary>
	/// <param name="grub">The grub that was hit.</param>
	protected virtual void HitGrub( Grub grub )
	{
		var dir = (grub.Position - Position).Normal;
		grub.ApplyAbsoluteImpulse( dir * 100 );

		grub.TakeDamage( new DamageInfo
		{
			Attacker = Parent,
			Damage = Damage,
			Flags = DamageFlags,
			Position = Position
		} );
	}

	/// <summary>
	/// Debug method to show information regarding a melee weapon.
	/// </summary>
	protected virtual void DebugDraw()
	{
		var holder = Parent as Grub;
		var mins = Vector3.Min( HitStart, HitStart + (holder!.FacingLeft ? HitSize.WithX( -HitSize.x ) : HitSize) );
		var maxs = Vector3.Max( HitStart, HitStart + (holder.FacingLeft ? HitSize.WithX( -HitSize.x ) : HitSize) );

		DebugOverlay.Box( mins, maxs, Color.Yellow, 5 );
	}

	/// <summary>
	/// Debug console variable to show the area that the melee weapon hits.
	/// </summary>
	[ConVar.Replicated( "melee_debug" )]
	public static bool MeleeDebug { get; set; }
}
