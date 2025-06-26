using System.Collections.Generic;
using Sandbox.Sdf;

namespace Sandbox;

public static class LayerUtility
{
	private static readonly Dictionary<string, Resource> LayerCache = new();

	public static void AddLayer( string layerId, Sdf2DLayer layer )
	{
		if ( layerId is null )
		{
			Log.Warning( $"Failed to add a layer to the cache: layerId was null" );
		}

		LayerCache.TryAdd( layerId, layer );
	}

	public static Resource Get( string layerId )
	{
		return LayerCache.GetValueOrDefault( layerId );
	}
}
