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
		var remove = ForceResolved?.ToList();
		if ( remove is null )
			return;

		if ( !Game.ActiveScene.IsValid() )
			return;
		
		foreach ( var r in remove )
		{
			var go = Game.ActiveScene.Directory.FindByGuid( r );
			if ( !go.IsValid() )
				continue;
			
			if ( !clearProjectiles && (go.Tags?.Has( "projectile" ) ?? false) )
				continue;

			ForceResolved?.Remove( r );
		}
	}

	public static async Task UntilWorldResolved( int maxRetries = -1 )
	{
		var retryCount = 0;

		if ( !Game.ActiveScene.IsValid() )
			return;

		while ( !IsWorldResolved() && retryCount++ < maxRetries )
		{
			if ( retryCount == maxRetries - 1 )
			{
				var unresolved = Game.ActiveScene.GetAllComponents<IResolvable>().Where( r => !ForceResolved.Contains( ((Component)r).GameObject.Id ) && !r.Resolved );
				Log.Warning( $"{unresolved.Count()} COMPONENTS ARE NOT RESOLVED!" );
				Log.Warning( "PLEASE REPORT THIS TO A DEVELOPER AT DISCORD.GG/APETAVERN!!!" );
				foreach ( var r in unresolved )
					Log.Warning( r );

				// "Force Resolve" unresolved components so we don't wait 15 seconds each time we watch a Grub take damage.
				foreach ( var u in unresolved )
					ForceResolveObject( ((Component)u).GameObject.Id );
			}

			await GameTask.DelayRealtime( 500 );
		}
	}
}

public interface IResolvable
{
	bool Resolved { get; }
}
