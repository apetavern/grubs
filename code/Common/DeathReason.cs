using Grubs.Pawn;

namespace Grubs.Common;

/// <summary>
/// Holds the data needed to find out how a grub has died.
/// </summary>
public readonly struct DeathReason
{
	/// <summary>
	/// The grub that died.
	/// </summary>
	public readonly Grub Grub;

	/// <summary>
	/// The first damage info responsible for the death.
	/// </summary>
	public readonly GrubsDamageInfo FirstInfo;

	/// <summary>
	/// The first reason for the death.
	/// </summary>
	public readonly DamageType FirstReason;

	/// <summary>
	/// The second damage info responsible for the death.
	/// </summary>
	public readonly GrubsDamageInfo SecondInfo;

	/// <summary>
	/// The second reason for the death.
	/// </summary>
	public readonly DamageType SecondReason;

	/// <summary>
	/// Returns whether or not the grub was killed by hitting a kill trigger.
	/// </summary>
	public bool FromKillTrigger => FirstReason == DamageType.KillZone || SecondReason == DamageType.KillZone;

	public bool FromDisconnect => FirstReason == DamageType.Disconnect || SecondReason == DamageType.Disconnect;

	public DeathReason( Grub grub, GrubsDamageInfo firstInfo, DamageType firstReason, GrubsDamageInfo secondInfo,
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
						return $"{Grub.Name} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{Grub.Name} had no reason left to live...";
					// Died from an explosion.
					case DamageType.Explosion:
						return SecondInfo?.Attacker == Grub
							? $"{Grub.Name} blew themselves up like an idiot."
							: $"{Grub.Name} was blown to bits by {SecondInfo.Attacker.Name}.";
					case DamageType.Fire:
						return SecondInfo?.Attacker == Grub
							? $"{Grub.Name} burned themselves to death."
							: $"{Grub.Name} was cooked by {SecondInfo.Attacker.Name}.";
					// Died from falling.
					case DamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Died from hitting a kill zone.
					case DamageType.KillZone:
						return $"{Grub.Name} escaped the simulation.";
					// Died from a HitScan weapon.
					case DamageType.HitScan:
						return $"{Grub.Name} was shot in the head.";
					// Died from a Melee weapon.
					case DamageType.Melee:
						return $"{Grub.Name} took a blunt object to the face.";
				}

				break;
			// Assisted by an explosion.
			case DamageType.Explosion:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{Grub.Name} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{Grub.Name} had no reason left to live...";
					// Killed by a different explosion.
					case DamageType.Explosion:
						return $"{Grub.Name} attracted too many explosives.";
					case DamageType.Fire:
						return $"{Grub.Name} perished in a firey explosion.";
					// Killed by a fall from being displaced by an explosion.
					case DamageType.Fall:
						return FirstInfo?.Attacker == Grub
							? $"{Grub.Name} had their leg broken thanks to their own explosives."
							: $"{Grub.Name} had their leg broken thanks to {FirstInfo.Attacker.Name}s explosive.";
					// Killed by hitting a kill zone from being displaced by an explosion.
					case DamageType.KillZone:
						return FirstInfo?.Attacker == Grub
							? $"{Grub.Name} sent themself to the shadow realm."
							: $"{Grub.Name} got sent to the shadow realm by {FirstInfo.Attacker.Name}.";
					// Killed from a HitScan weapon after an explosion (this shouldn't happen).
					case DamageType.HitScan:
						return $"{Grub.Name} has experienced a series of unfortunate events.";
					// Killed from a Melee weapon after an explosion (this shouldn't happen).
					case DamageType.Melee:
						return $"{Grub.Name} shouldn't have skipped gym class.";
				}

				break;
			// Assisted by a fire.
			case DamageType.Fire:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{Grub.Name} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{Grub.Name} burnt their will to play.";
					// Killed by an explosion.
					case DamageType.Explosion:
						return $"{Grub.Name} perished in a fiery explosion.";
					case DamageType.Fire:
						return $"{Grub.Name} burnt to a crisp.";
					// Killed by a fall from being displaced by fire.
					case DamageType.Fall:
						return FirstInfo?.Attacker == Grub
							? $"{Grub.Name} had their leg broken thanks to their own explosives."
							: $"{Grub.Name} had their leg broken thanks to {FirstInfo.Attacker.Name}s explosive.";
					// Killed by hitting a kill zone from being displaced by fire.
					case DamageType.KillZone:
						return FirstInfo?.Attacker == Grub
							? $"{Grub.Name} went from too hot to too cold."
							: $"{Grub.Name} went through the temperature spectrum thanks to {FirstInfo.Attacker.Name}.";
					// Killed from a HitScan weapon after fire (this shouldn't happen).
					case DamageType.HitScan:
						return $"{Grub.Name} was shot and then set on fire.";
					// Killed from a Melee weapon after fire (this shouldn't happen).
					case DamageType.Melee:
						return $"{Grub.Name} was knocked into the sun.";
				}

				break;
			// Assisted by a fall.
			case DamageType.Fall:
				switch ( SecondReason )
				{
					// Fell into an explosion.
					case DamageType.Explosion:
						return $"{Grub.Name} had an unlucky fall.";
					case DamageType.Fire:
						return $"{Grub.Name} fell into a pit of fire.";
					// Fell into a fall (this shouldn't happen).
					case DamageType.Fall:
						return $"{Grub.Name} broke their... leg?";
					// Fell into a kill zone (this shouldn't happen).
					case DamageType.KillZone:
						return $"{Grub.Name} escaped the simulation.";
					// Fell into a HitScan weapon.
					case DamageType.HitScan:
						return $"{Grub.Name} was sniped in mid-air.";
					// Fell into a Melee weapon hit (this shouldn't happen).
					case DamageType.Melee:
						return $"{Grub.Name} got kicked in the stomach.";
				}

				break;
			// Shot by a HitScan weapon.
			case DamageType.HitScan:
				switch ( SecondReason )
				{
					case DamageType.Explosion:
						return $"{Grub.Name} got shot so hard they blew up.";
					case DamageType.Fire:
						return $"{Grub.Name} was shot and then set on fire.";
					case DamageType.Fall:
						return $"{Grub.Name} suffered an unfortunate fall.";
					case DamageType.KillZone:
						return $"{Grub.Name} was no-scoped into hell.";
					case DamageType.HitScan:
						return $"{Grub.Name} was made into swiss cheese.";
					case DamageType.Melee:
						return $"{Grub.Name} was pulverized.";
				}

				break;
			// Hit with a Melee weapon.
			case DamageType.Melee:
				switch ( SecondReason )
				{
					case DamageType.Explosion:
						return $"{Grub.Name} was punted into an explosive situation.";
					case DamageType.Fire:
						return $"{Grub.Name} was knocked into the sun.";
					case DamageType.Fall:
						return $"{Grub.Name} got Sparta kicked.";
					case DamageType.KillZone:
						return $"{Grub.Name} was sent to the Nether.";
					case DamageType.HitScan:
						return $"{Grub.Name} is definitely dead.";
					case DamageType.Melee:
						return $"{Grub.Name} got fisted.";
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
	public static DeathReason FindReason( Grub grub, List<GrubsDamageInfo> damageInfos )
	{
		GrubsDamageInfo lastReasonInfo = null;
		var lastReason = DamageType.None;
		GrubsDamageInfo reasonInfo = null;
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
					// Controlling player disconnected.
					case "disconnect":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Disconnect;
						break;
					// An explosion, just move the reasons around as normal.
					case "explosion":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Explosion;
						break;
					case "fire":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Fire;
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
					case "hitscan":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.HitScan;
						break;
					case "melee":
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.Melee;
						break;
					// Hit a kill trigger.
					case "killzone":
						// If we got to the kill trigger from falling from a great height then just overwrite it.
						if ( reason == DamageType.Fall )
						{
							reasonInfo = damageInfo;
							reason = DamageType.KillZone;
							break;
						}

						// Move as normal.
						lastReasonInfo = reasonInfo;
						lastReason = reason;
						reasonInfo = damageInfo;
						reason = DamageType.KillZone;
						break;
				}
			}
		}

		return new DeathReason( grub, lastReasonInfo, lastReason, reasonInfo, reason );
	}
}
