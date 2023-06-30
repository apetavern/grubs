namespace Grubs;

public partial class Terrain
{
	/// <summary>
	/// Whether all IResolvable entities are resolved or not.
	/// </summary>
	/// <returns>True, if all IResolvable entities are resolved.</returns>
	public static bool IsResolved()
	{
		return All.OfType<IResolvable>().All( ent => ent.Resolved );
	}

	/// <summary>
	/// A helper function to asynchronously check whether all entities are resolved.
	/// </summary>
	/// <param name="maxRetries">An optional argument to specify a max amount of retries for resolving.
	/// If left empty, the operation continues indefinitely.</param>
	/// <returns>An asynchronous task.</returns>
	public static async Task UntilResolve( int maxRetries = -1 )
	{
		var retryCount = 0;

		if ( retryCount > 10 )
		{
			var unresolved = All.OfType<IResolvable>().Where( ent => !ent.Resolved );
			Log.Warning( $"{unresolved.Count()} ENTITIES ARE NOT RESOLVED" );
			Log.Warning( $"PLEASE REPORT THIS TO A DEVELOPER AT DISCORD.GG/APETAVERN!!!" );
			foreach ( var r in unresolved )
			{
				if ( r is not Entity e )
					return;
				Log.Warning( e.Name );
			}
		}

		while ( !IsResolved() || retryCount++ < maxRetries )
			await GameTask.Delay( 300 );
	}
}
