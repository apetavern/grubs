namespace Grubs;

public static class ListExtensions
{
	public static int HashCombine<T>( this IEnumerable<T> e, Func<T, decimal> selector )
	{
		var result = 0;

		foreach ( var el in e )
			result = HashCode.Combine( result, selector.Invoke( el ) );

		return result;
	}

	public static void Simulate<T>( this IList<T> list, IClient client ) where T : Entity
	{
		for ( int i = list.Count - 1; i >= 0; --i )
		{
			var entity = list.ElementAt( i );
			if ( entity.IsValid() )
				entity.Simulate( client );
			else
				list.RemoveAt( i );
		}
	}

	public static IEnumerable<T> Shuffle<T>( this IEnumerable<T> list )
	{
		return list.OrderBy( x => Guid.NewGuid() );
	}
}
