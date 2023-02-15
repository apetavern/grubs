namespace Grubs;

/// <summary>
/// Holds the data needed to find out how a grub has died.
/// </summary>
public struct DeathReason
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
	public readonly DamageType FirstReason;
	/// <summary>
	/// The second damage info responsible for the death.
	/// </summary>
	public readonly DamageInfo? SecondInfo;
	/// <summary>
	/// The second reason for the death.
	/// </summary>
	public readonly DamageType SecondReason;

	/// <summary>
	/// Returns whether or not the grub was killed by hitting a kill trigger.
	/// </summary>
	public bool FromKillTrigger => FirstReason == DamageType.KillTrigger || SecondReason == DamageType.KillTrigger;

	public DeathReason( Grub grub, DamageInfo? firstInfo, DamageType firstReason, DamageInfo? secondInfo,
		DamageType secondReason )
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
			case DamageType.None:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{Grub.Name} was RDMed by an admin";
					// Died from an explosion.
					case DamageType.Explosion:
						return SecondInfo!.Value.Attacker == Grub
							? $"{Grub.Name} blew themselves up like an idiot"
							: $"{Grub.Name} was blown to bits by {SecondInfo.Value.Attacker.Name}";
					// Died from falling.
					case DamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Died from hitting a kill zone.
					case DamageType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
			// Assisted by an explosion.
			case DamageType.Explosion:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{Grub.Name} was RDMed by an admin";
					// Killed by a different explosion.
					case DamageType.Explosion:
						return $"{Grub.Name} attracted too many explosives";
					// Killed by a fall from being displaced by an explosion.
					case DamageType.Fall:
						return FirstInfo!.Value.Attacker == Grub
							? $"{Grub.Name} had their leg broken thanks to their own explosives"
							: $"{Grub.Name} had their leg broken thanks to {FirstInfo!.Value.Attacker.Name}s explosive";
					// Killed by hitting a kill zone from being displaced by an explosion.
					case DamageType.KillTrigger:
						return FirstInfo!.Value.Attacker == Grub
							? $"{Grub.Name} sent themself to the shadow realm"
							: $"{Grub.Name} got sent to the shadow realm by {FirstInfo.Value.Attacker.Name}";
				}
				break;
			// Assisted by a fall.
			case DamageType.Fall:
				switch ( SecondReason )
				{
					// Fell into an explosion.
					case DamageType.Explosion:
						return $"{Grub.Name} had an unlucky fall";
					// Fell into a fall (this shouldn't happen).
					case DamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Fell into a kill zone (this shouldn't happen).
					case DamageType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
		}

		// Failed to find the right death reason.
		return $"Who knows what the fuck happened to {Grub.Name}";
	}

	/// <summary>
	/// Parses a set of damage info and creates a reason for a grubs death.
	/// </summary>
	/// <param name="grub">The grub that is dying.</param>
	/// <param name="damageInfos">The damage info the grub received.</param>
	/// <returns></returns>
	public static DeathReason FindReason( Grub grub, List<DamageInfo> damageInfos )
	{
		DamageInfo? lastReasonInfo = null;
		var lastReason = DamageType.None;
		DamageInfo? reasonInfo = null;
		var reason = DamageType.None;

		foreach ( var damageInfo in damageInfos )
		{
			foreach ( var tag in damageInfo.Tags )
			{
				switch ( tag )
				{
					// An admin has abused the grub, move as normal.
					case "admin":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Admin;
						break;
					// An explosion, just move the reasons around as normal.
					case "explosion":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Explosion;
						break;
					// Fell from a great height. Only move the reasons around if we weren't already falling.
					case "fall":
						if ( reason == DamageType.Fall )
							break;

						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Fall;
						break;
					// Hit a kill trigger.
					case "outofarea":
						// If we got to the kill trigger from falling from a great height then just overwrite it.
						if ( reason == DamageType.Fall )
						{
							reasonInfo = damageInfo;
							reason = DamageType.KillTrigger;
							break;
						}

						// Move as normal.
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.KillTrigger;
						break;
				}
			}
		}

		return new DeathReason( grub, lastReasonInfo, lastReason, reasonInfo, reason );
	}
}
