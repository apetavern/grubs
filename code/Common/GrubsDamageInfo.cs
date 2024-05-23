namespace Grubs.Common;

public sealed class GrubsDamageInfo
{
	public Vector3 WorldPosition { get; set; }
	public GameObject Attacker { get; set; }
	public GameObject Weapon { get; set; }
	public float Damage { get; set; }
	public TagSet Tags { get; set; }

	public GrubsDamageInfo( float damage, GameObject attacker = null, GameObject weapon = null, Vector3 worldPosition = new Vector3() )
	{
		Damage = damage;
		Attacker = attacker;
		Weapon = weapon;
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

	public static GrubsDamageInfo FromHitscan( float damage, GameObject attacker = null, GameObject weapon = null, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, weapon, worldPosition ).WithTag( "hitscan" );
	}

	public static GrubsDamageInfo FromMelee( float damage, GameObject attacker = null, GameObject weapon = null, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, weapon, worldPosition ).WithTag( "melee" );
	}

	public static GrubsDamageInfo FromExplosion( float damage, GameObject attacker = null, GameObject weapon = null, Vector3 worldPosition = new Vector3() )
	{
		return new GrubsDamageInfo( damage, attacker, weapon, worldPosition ).WithTag( "explosion" );
	}

	public static GrubsDamageInfo FromKillZone( float damage )
	{
		return new GrubsDamageInfo( damage ).WithTag( "killzone" );
	}

	public static GrubsDamageInfo FromFall( float damage )
	{
		return new GrubsDamageInfo( damage ).WithTag( "fall" );
	}
}
