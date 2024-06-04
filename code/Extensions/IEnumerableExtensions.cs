namespace Grubs.Extensions;

public static class ListExtensions
{
	public static int HashCombine<T>( this IEnumerable<T> e, Func<T, decimal> selector )
	{
		var result = 0;

		foreach ( var e1 in e )
			result = HashCode.Combine( result, selector.Invoke( e1 ) );

		return result;
	}

	public static void Shuffle<T>( this IList<T> list )
	{
		var n = list.Count;
		while ( n > 1 )
		{
			n--;
			var k = Random.Shared.Next( n + 1 );
			(list[n], list[k]) = (list[k], list[n]);
		}
	}
}
