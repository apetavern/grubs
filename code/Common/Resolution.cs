namespace Grubs.Common;

public static class Resolution
{
	public static bool IsWorldResolved() => Game.ActiveScene.GetAllComponents<IResolvable>().Where( r => !ForceResolved.Contains( (r as Component).GameObject.Id ) ).All( r => r.Resolved );

	// List<GameObject>
	public static List<Guid> ForceResolved { get; } = new();

	[Broadcast]
	public static void ForceResolveObject( Guid objectId ) => ForceResolved.Add( objectId );

	[Broadcast]
	public static void ClearForceResolved( bool clearProjectiles = true )
	{
		// Deep copy to avoid InvalidOperationException.
		var remove = ForceResolved.ToList();
		foreach ( var r in remove )
		{
			if ( !clearProjectiles && Game.ActiveScene.Directory.FindByGuid( r ).Tags.Has( "projectile" ) )
				continue;

			ForceResolved.Remove( r );
		}
	}

	public static async Task UntilWorldResolved( int maxRetries = -1 )
	{
		var retryCount = 0;

		while ( !IsWorldResolved() && retryCount++ < maxRetries )
		{
			if ( retryCount == maxRetries - 1 )
			{
				var unresolved = Game.ActiveScene.GetAllComponents<IResolvable>().Where( r => !ForceResolved.Contains( (r as Component).GameObject.Id ) && !r.Resolved );
				Log.Warning( $"{unresolved.Count()} COMPONENTS ARE NOT RESOLVED!" );
				Log.Warning( "PLEASE REPORT THIS TO A DEVELOPER AT DISCORD.GG/APETAVERN!!!" );
				foreach ( var r in unresolved )
					Log.Warning( r );

				// "Force Resolve" unresolved components so we don't wait 15 seconds each time we watch a Grub take damage.
				foreach ( var u in unresolved )
					ForceResolveObject( (u as Component).GameObject.Id );
			}

			await GameTask.DelayRealtime( 500 );
		}
	}
}

public interface IResolvable
{
	bool Resolved { get; }
}
