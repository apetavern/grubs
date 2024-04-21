﻿using Grubs.Pawn;

namespace Grubs.Common;

[Title( "Grubs - Kill Zone" ), Category( "Grubs" )]
public class KillZoneComponent : Component, Component.ITriggerListener
{
	public void OnTriggerEnter( Collider other )
	{
		if ( other.GameObject.Components.TryGet( out Grub grub, FindMode.EverythingInSelfAndAncestors ) )
			grub.Health.TakeDamage( 9999, true );
	}

	public void OnTriggerExit( Collider other )
	{
	}
}
