using Grubs.Pawn;

namespace Grubs.Common;

public sealed class GrubsDamageInfo
{
	public Vector3 WorldPosition { get; set; }
	public Grub Attacker { get; set; }
	public float Damage { get; set; }
	public TagSet Tags { get; set; }

	public GrubsDamageInfo( float damage, Grub attacker = null, Vector3 worldPosition = new Vector3() )
	{
		Damage = damage;
		Attacker = attacker;
		WorldPosition = worldPosition;
		Tags = new TagSet();
	}

	public GrubsDamageInfo WithTag( string tag )
	{
		if ( !Tags.Has( tag ) )
			Tags.Add( tag );

		return this;
	}

	public GrubsDamageInfo WithTags( params string[] tags )
	{
		var info = this;
		foreach ( var tag in tags )
			info = info.WithTag( tag );

		return info;
	}

	public static GrubsDamageInfo FromHitscan( float damage, Grub attacker, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, worldPosition ).WithTag( "hitscan" );
	}

	public static GrubsDamageInfo FromMelee( float damage, Grub attacker, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, worldPosition ).WithTag( "melee" );
	}

	public static GrubsDamageInfo FromExplosion( float damage, Grub attacker, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, worldPosition ).WithTag( "explosion" );
	}

	public static GrubsDamageInfo FromFire( float damage, Grub attacker, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, worldPosition ).WithTag( "fire" );
	}

	public static GrubsDamageInfo FromFall( float damage, Grub attacker )
	{
		return new GrubsDamageInfo( damage, attacker ).WithTag( "fall" );
	}

	public static GrubsDamageInfo FromKillZone( float damage )
	{
		return new GrubsDamageInfo( damage ).WithTag( "killzone" );
	}
}
