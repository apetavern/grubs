using Grubs.Pawn;

namespace Grubs.Extensions;

public static class GuidExtensions
{
	public static T? ToComponent<T>( this Guid guid ) where T : Component
	{
		return Game.ActiveScene.Directory.FindComponentByGuid( guid ) as T;
	}
}
