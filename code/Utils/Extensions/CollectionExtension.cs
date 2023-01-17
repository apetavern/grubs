using System.Diagnostics;

namespace Grubs.Utils.Extensions;

public static class CollectionExtension
{
	public static T GetByIndex<T>( this IReadOnlyCollection<T> collection, int index )
	{
		if ( index >= collection.Count || index < 0 )
			throw new IndexOutOfRangeException( $"Index {index} is out of range of the collection (Count = {collection.Count})" );

		var i = 0;
		foreach ( var item in collection )
		{
			if ( i == index )
				return item;

			i++;
		}

		throw new UnreachableException();
	}
}
