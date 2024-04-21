using Grubs.Common;

namespace Grubs.Terrain;

public partial class GrubsTerrain
{
	public static bool IsResolved() => Game.ActiveScene.GetAllComponents<IResolvable>().All( r => r.Resolved );

	public static async Task UntilResolve( int maxRetries = -1 )
	{
		var retryCount = 0;

		while ( !IsResolved() && retryCount++ < maxRetries )
		{
			if ( retryCount > 10 )
			{
				var unresolved = Game.ActiveScene.GetAllComponents<IResolvable>().Where( r => !r.Resolved );
				Log.Warning( $"{unresolved.Count()} GAMEOBJECTS ARE NOT RESOLVED!" );
				Log.Warning( "PLEASE REPORT THIS TO A DEVELOPER AT DISCORD.GG/APETAVERN!!!" );
				foreach ( var r in unresolved )
					Log.Warning( r );
			}

			await GameTask.Delay( 500 );
		}
	}
}
