namespace Grubs.Common;

public static class Resolution
{
	public static bool IsWorldResolved() => Game.ActiveScene.GetAllComponents<IResolvable>().All( r => r.Resolved );

	public static async Task UntilWorldResolved( int maxRetries = -1 )
	{
		var retryCount = 0;

		while ( !IsWorldResolved() && retryCount++ < maxRetries )
		{
			if ( retryCount > 20 )
			{
				var unresolved = Game.ActiveScene.GetAllComponents<IResolvable>().Where( r => !r.Resolved );
				Log.Warning( $"{unresolved.Count()} COMPONENTS ARE NOT RESOLVED!" );
				Log.Warning( "PLEASE REPORT THIS TO A DEVELOPER AT DISCORD.GG/APETAVERN!!!" );
				foreach ( var r in unresolved )
					Log.Warning( r );
			}

			await GameTask.DelayRealtime( 500 );
		}
	}
}

public interface IResolvable
{
	bool Resolved { get; }
}
