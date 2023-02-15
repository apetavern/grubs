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
}
