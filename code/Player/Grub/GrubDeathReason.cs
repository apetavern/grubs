namespace Grubs.Player;

public readonly struct GrubDeathReason
{
	public readonly Grub Grub;
	public readonly DamageInfo? FirstInfo;
	public readonly GrubDeathReasonType FirstReason;
	public readonly DamageInfo? SecondInfo;
	public readonly GrubDeathReasonType SecondReason;

	public GrubDeathReason( Grub grub, DamageInfo? firstInfo, GrubDeathReasonType firstReason, DamageInfo? secondInfo,
		GrubDeathReasonType secondReason )
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
			case GrubDeathReasonType.None:
				switch ( SecondReason )
				{
					case GrubDeathReasonType.Explosion:
						return SecondInfo.Value.Attacker == Grub
							? $"{Grub.Name} blew themselves up like an idiot"
							: $"{Grub.Name} was blown to bits by {SecondInfo.Value.Attacker.Name}";

					case GrubDeathReasonType.Fall:
						return $"{Grub.Name} broke their... leg?";
					case GrubDeathReasonType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
			case GrubDeathReasonType.Explosion:
				switch ( SecondReason )
				{
					case GrubDeathReasonType.Explosion:
						return $"{Grub.Name} attracted too many explosives";
					case GrubDeathReasonType.Fall:
						return $"{Grub.Name} had their leg broken by {FirstInfo.Value.Attacker.Name}s explosive";
					case GrubDeathReasonType.KillTrigger:
						return SecondInfo.Value.Attacker == Grub
							? $"{Grub.Name} sent themself to the shadow realm"
							: $"{Grub.Name} got sent to the shadow realm by {SecondInfo.Value.Attacker.Name}";
				}
				break;
			case GrubDeathReasonType.Fall:
				switch ( SecondReason )
				{
					case GrubDeathReasonType.Explosion:
						return $"{Grub.Name} had an unlucky fall";
					case GrubDeathReasonType.Fall:
						return $"{Grub.Name} broke their... leg?";
					case GrubDeathReasonType.KillTrigger:
						return $"{Grub.Name} escaped the simulation";
				}
				break;
		}

		return "Who knows what the fuck happened";
	}

	public static GrubDeathReason FindReason( Grub grub, List<DamageInfo> damageInfos )
	{
		DamageInfo? lastReasonInfo = null;
		var lastReason = GrubDeathReasonType.None;
		DamageInfo? reasonInfo = null;
		var reason = GrubDeathReasonType.None;

		foreach ( var damageInfo in damageInfos )
		{
			switch ( damageInfo.Flags )
			{
				case DamageFlags.Blast:
					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDeathReasonType.Explosion;
					break;
				case DamageFlags.Fall:
					if ( reason == GrubDeathReasonType.Fall )
						break;

					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDeathReasonType.Fall;
					break;
				case DamageFlags.Generic:
					if ( reason == GrubDeathReasonType.Fall )
					{
						reasonInfo = damageInfo;
						reason = GrubDeathReasonType.KillTrigger;
						break;
					}

					lastReasonInfo = reasonInfo;
					lastReason = reason;
					reasonInfo = damageInfo;
					reason = GrubDeathReasonType.KillTrigger;
					break;
			}
		}

		return new GrubDeathReason( grub, lastReasonInfo, lastReason, reasonInfo, reason );
	}
}
