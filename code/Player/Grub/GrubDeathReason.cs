namespace Grubs.Player;

/// <summary>
/// Holds the data needed to find out how a grub has died.
/// </summary>
public readonly struct GrubDeathReason
{
	/// <summary>
	/// The grub that died.
	/// </summary>
	public readonly Grub Grub;
	/// <summary>
	/// The first damage info responsible for the death.
	/// </summary>
	public readonly DamageInfo? FirstInfo;
	/// <summary>
	/// The first reason for the death.
	/// </summary>
	public readonly GrubDamageType FirstReason;
	/// <summary>
	/// The second damage info responsible for the death.
	/// </summary>
	public readonly DamageInfo? SecondInfo;
	/// <summary>
	/// The second reason for the death.
	/// </summary>
	public readonly GrubDamageType SecondReason;

	public GrubDeathReason( Grub grub, DamageInfo? firstInfo, GrubDamageType firstReason, DamageInfo? secondInfo,
		GrubDamageType secondReason )
	{
		Grub = grub;
		FirstInfo = firstInfo;
		FirstReason = firstReason;
		SecondInfo = secondInfo;
		SecondReason = secondReason;
	}

	public override string ToString()
	{
		switch ( FirstReason )
		{
			// Only one thing killed the grub.
			case GrubDamageType.None:
				switch ( SecondReason )
				{
					// Died from an explosion.
					case GrubDamageType.Explosion:
						return SecondInfo.Value.Attacker == Grub
							? $"{Grub.Name} blew themselves up like an idiot"
							: $"{Grub.Name} was blown to bits by {SecondInfo.Value.Attacker.Name}";
					// Died from falling.
					case GrubDamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Died from hitting a kill zone.
					case GrubDamageType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
			// Assisted by an explosion.
			case GrubDamageType.Explosion:
				switch ( SecondReason )
				{
					// Killed by a different explosion.
					case GrubDamageType.Explosion:
						return $"{Grub.Name} attracted too many explosives";
					// Killed by a fall from being displaced by an explosion.
					case GrubDamageType.Fall:
						return $"{Grub.Name} had their leg broken thanks to {FirstInfo.Value.Attacker.Name}s explosive";
					// Killed by hitting a kill zone from being displaced by an explosion.
					case GrubDamageType.KillTrigger:
						return SecondInfo.Value.Attacker == Grub
							? $"{Grub.Name} sent themself to the shadow realm"
							: $"{Grub.Name} got sent to the shadow realm by {SecondInfo.Value.Attacker.Name}";
				}
				break;
			// Assisted by a fall.
			case GrubDamageType.Fall:
				switch ( SecondReason )
				{
					// Fell into an explosion.
					case GrubDamageType.Explosion:
						return $"{Grub.Name} had an unlucky fall";
					// Fell into a fall (this shouldn't happen).
					case GrubDamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Fell into a kill zone (this shouldn't happen).
					case GrubDamageType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
		}

		// Failed to find the right death reason.
		return "Who knows what the fuck happened";
	}

	/// <summary>
	/// Parses a set of damage info and creates a reason for a grubs death.
	/// </summary>
	/// <param name="grub">The grub that is dying.</param>
	/// <param name="damageInfos">The damage info the grub received.</param>
	/// <returns></returns>
	public static GrubDeathReason FindReason( Grub grub, List<DamageInfo> damageInfos )
	{
		DamageInfo? lastReasonInfo = null;
		var lastReason = GrubDamageType.None;
		DamageInfo? reasonInfo = null;
		var reason = GrubDamageType.None;

		foreach ( var damageInfo in damageInfos )
		{
			switch ( damageInfo.Flags )
			{
				// An explosion, just move the reasons around as normal.
				case DamageFlags.Blast:
					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDamageType.Explosion;
					break;
				// Fell from a great height. Only move the reasons around if we weren't already falling.
				case DamageFlags.Fall:
					if ( reason == GrubDamageType.Fall )
						break;

					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDamageType.Fall;
					break;
				// Hit a kill trigger.
				case DamageFlags.Generic:
					// If we got to the kill trigger from falling from a great height then just overwrite it.
					if ( reason == GrubDamageType.Fall )
					{
						reasonInfo = damageInfo;
						reason = GrubDamageType.KillTrigger;
						break;
					}

					// Move as normal.
					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDamageType.KillTrigger;
					break;
			}
		}

		return new GrubDeathReason( grub, lastReasonInfo, lastReason, reasonInfo, reason );
	}
}
