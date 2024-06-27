using Grubs.Pawn;

namespace Grubs.Common;

/// <summary>
/// Holds the data needed to find out how a grub has died.
/// </summary>
public readonly struct DeathReason
{
	/// <summary>
	/// The GUID of the Grub that died.
	/// </summary>
	public readonly Guid GrubGuid;

	/// <summary>
	/// The name of the Grub that died.
	/// </summary>
	public readonly string GrubName;

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
	/// Whether or not the grub was killed by hitting a kill trigger.
	/// </summary>
	public bool FromKillTrigger => FirstReason == DamageType.KillZone || SecondReason == DamageType.KillZone;

	/// <summary>
	/// Whether or not the grub was killed from their player disconnecting.
	/// </summary>
	public bool FromDisconnect => FirstReason == DamageType.Disconnect || SecondReason == DamageType.Disconnect;

	public DeathReason( Guid grubGuid, string grubName, GrubsDamageInfo firstInfo, DamageType firstReason, GrubsDamageInfo secondInfo,
		DamageType secondReason )
	{
		GrubGuid = grubGuid;
		GrubName = grubName;
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
						return $"{GrubName} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{GrubName} had no reason left to live...";
					// Died from an explosion.
					case DamageType.Explosion:
						return SecondInfo.AttackerGuid == Guid.Empty || SecondInfo.AttackerGuid == GrubGuid
							? $"{GrubName} blew themselves up like an idiot."
							: $"{GrubName} was blown to bits by {SecondInfo.AttackerName}.";
					case DamageType.Fire:
						return SecondInfo.AttackerGuid == Guid.Empty || SecondInfo.AttackerGuid == GrubGuid
							? $"{GrubName} burned themselves to death."
							: $"{GrubName} was cooked by {SecondInfo.AttackerName}.";
					// Died from falling.
					case DamageType.Fall:
						return $"{GrubName} broke their... leg?";
					// Died from hitting a kill zone.
					case DamageType.KillZone:
						return $"{GrubName} escaped the simulation.";
					// Died from a HitScan weapon.
					case DamageType.HitScan:
						return $"{GrubName} was shot in the head by {SecondInfo.AttackerName}.";
					// Died from a Melee weapon.
					case DamageType.Melee:
						return $"{GrubName} took a blunt object to the face from {SecondInfo.AttackerName}.";
				}

				break;
			// Assisted by an explosion.
			case DamageType.Explosion:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{GrubName} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{GrubName} had no reason left to live...";
					// Killed by a different explosion.
					case DamageType.Explosion:
						return $"{GrubName} attracted too many explosives.";
					case DamageType.Fire:
						return $"{GrubName} perished in a firey explosion.";
					// Killed by a fall from being displaced by an explosion.
					case DamageType.Fall:
						return FirstInfo.AttackerGuid == Guid.Empty || FirstInfo.AttackerGuid == GrubGuid
							? $"{GrubName} had their leg broken thanks to their own explosives."
							: $"{GrubName} had their leg broken thanks to {FirstInfo.AttackerName}'s explosive.";
					// Killed by hitting a kill zone from being displaced by an explosion.
					case DamageType.KillZone:
						return FirstInfo.AttackerGuid == Guid.Empty || FirstInfo.AttackerGuid == GrubGuid
							? $"{GrubName} sent themself to the shadow realm."
							: $"{GrubName} got sent to the shadow realm by {FirstInfo.AttackerName}.";
					// Killed from a HitScan weapon after an explosion (this shouldn't happen).
					case DamageType.HitScan:
						return $"{GrubName} has experienced a series of unfortunate events.";
					// Killed from a Melee weapon after an explosion (this shouldn't happen).
					case DamageType.Melee:
						return $"{GrubName} shouldn't have skipped gym class.";
				}

				break;
			// Assisted by a fire.
			case DamageType.Fire:
				switch ( SecondReason )
				{
					// Died from an admin.
					case DamageType.Admin:
						return $"{GrubName} was RDMed by an admin!";
					// Controlling player disconnected.
					case DamageType.Disconnect:
						return $"{GrubName} burnt their will to play.";
					// Killed by an explosion.
					case DamageType.Explosion:
						return $"{GrubName} perished in a fiery explosion.";
					case DamageType.Fire:
						return $"{GrubName} burnt to a crisp.";
					// Killed by a fall from being displaced by fire.
					case DamageType.Fall:
						return $"{GrubName} had their legs broken thanks to their burns.";
					// Killed by hitting a kill zone from being displaced by fire.
					case DamageType.KillZone:
						return $"{GrubName} went through the temperature spectrum.";
					// Killed from a HitScan weapon after fire.
					case DamageType.HitScan:
						return $"{GrubName} was set on fire and shot by {SecondInfo.AttackerName}.";
					// Killed from a Melee weapon after fire (this shouldn't happen).
					case DamageType.Melee:
						return $"{GrubName} was knocked into the sun.";
				}

				break;
			// Assisted by a fall.
			case DamageType.Fall:
				switch ( SecondReason )
				{
					case DamageType.Disconnect:
						return $"{GrubName} fell into a pit of despair...";
					// Fell into an explosion.
					case DamageType.Explosion:
						return $"{GrubName} had an unlucky fall.";
					case DamageType.Fire:
						return $"{GrubName} fell into a pit of fire.";
					// Fell into a fall.
					case DamageType.Fall:
						return $"{GrubName} broke their... leg?";
					// Fell into a kill zone.
					case DamageType.KillZone:
						return $"{GrubName} escaped the simulation.";
					// Fell into a HitScan weapon.
					case DamageType.HitScan:
						return $"{GrubName} was sniped in mid-air.";
					// Fell into a Melee weapon hit (this shouldn't happen).
					case DamageType.Melee:
						return $"{GrubName} got kicked in the stomach.";
				}

				break;
			// Shot by a HitScan weapon.
			case DamageType.HitScan:
				switch ( SecondReason )
				{
					case DamageType.Disconnect:
						return $"{GrubName} was shot in the heart.";
					case DamageType.Explosion:
						return $"{GrubName} got shot by {FirstInfo.AttackerName} so hard they blew up.";
					case DamageType.Fire:
						return $"{GrubName} was shot by {FirstInfo.AttackerName} and then set on fire.";
					case DamageType.Fall:
						return $"{GrubName} suffered an unfortunate fall by the hands of {FirstInfo.AttackerName}.";
					case DamageType.KillZone:
						return $"{GrubName} was no-scoped into hell by {FirstInfo.AttackerName}.";
					case DamageType.HitScan:
						return $"{GrubName} was made into swiss cheese by {FirstInfo.AttackerName}.";
					case DamageType.Melee:
						return $"{GrubName} was pulverized.";
				}

				break;
			// Hit with a Melee weapon.
			case DamageType.Melee:
				switch ( SecondReason )
				{
					case DamageType.Disconnect:
						return $"{GrubName} was punched into oblivion.";
					case DamageType.Explosion:
						return $"{GrubName} was punted into an explosive situation by {FirstInfo.AttackerName}.";
					case DamageType.Fire:
						return $"{GrubName} was knocked into the sun by {FirstInfo.AttackerName}.";
					case DamageType.Fall:
						return $"{GrubName} got Sparta kicked by {FirstInfo.AttackerName}.";
					case DamageType.KillZone:
						return $"{GrubName} was sent to the Nether by {FirstInfo.AttackerName}.";
					case DamageType.HitScan:
						return $"{GrubName} is definitely dead.";
					case DamageType.Melee:
						return $"{GrubName} got fisted by {FirstInfo.AttackerName}.";
				}

				break;
		}

		// Failed to find the right death reason.
		return $"Who knows what the fuck happened to {GrubName}";
	}

	/// <summary>
	/// Parses a set of damage info and creates a reason for a grubs death.
	/// </summary>
	/// <param name="grub">The grub that is dying.</param>
	/// <param name="damageInfos">The damage info the grub received.</param>
	/// <returns></returns>
	public static DeathReason FindReason( Grub grub, List<GrubsDamageInfo> damageInfos )
	{
		GrubsDamageInfo lastReasonInfo = new();
		var lastReason = DamageType.None;
		GrubsDamageInfo reasonInfo = new();
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

		return new DeathReason( grub.Id, grub.Name, lastReasonInfo, lastReason, reasonInfo, reason );
	}
}
