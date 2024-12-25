namespace Grubs.Common;

public readonly struct GrubsDamageInfo
{
	public readonly Vector3 WorldPosition;
	public readonly Guid AttackerGuid;
	public readonly string AttackerName;
	public readonly float Damage;
	public readonly TagSet Tags = new();

	public GrubsDamageInfo( float damage, Guid attackerGuid, string attackerName = "", Vector3 worldPosition = new() )
	{
		Damage = damage;
		AttackerGuid = attackerGuid;
		AttackerName = attackerName;
		WorldPosition = worldPosition;
		Tags = new();
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

	public static GrubsDamageInfo FromHitscan( float damage, Guid attackerGuid, string attackerName, Vector3 worldPosition = new() )
	{
		return new GrubsDamageInfo( damage, attackerGuid, attackerName, worldPosition ).WithTag( "hitscan" );
	}

	public static GrubsDamageInfo FromMelee( float damage, Guid attackerGuid, string attackerName, Vector3 worldPosition = new() )
	{
		return new GrubsDamageInfo( damage, attackerGuid, attackerName, worldPosition ).WithTag( "melee" );
	}

	public static GrubsDamageInfo FromExplosion( float damage, Guid attackerGuid, string attackerName = "", Vector3 worldPosition = new() )
	{
		return new GrubsDamageInfo( damage, attackerGuid, attackerName, worldPosition ).WithTag( "explosion" );
	}

	public static GrubsDamageInfo FromFire( float damage, Guid attackerGuid, string attackerName = "", Vector3 worldPosition = new() )
	{
		return new GrubsDamageInfo( damage, attackerGuid, attackerName, worldPosition ).WithTag( "fire" );
	}

	public static GrubsDamageInfo FromFall( float damage, Guid attackerGuid, string attackerName )
	{
		return new GrubsDamageInfo( damage, attackerGuid, attackerName ).WithTag( "fall" );
	}

	public static GrubsDamageInfo FromKillZone( Vector3 position )
	{
		return new GrubsDamageInfo( float.MaxValue, Guid.Empty, worldPosition: position ).WithTag( "killzone" );
	}

	public static GrubsDamageInfo FromDisconnect()
	{
		return new GrubsDamageInfo( 9999, Guid.Empty ).WithTag( "disconnect" );
	}
}
