namespace Grubs.Utils;

/// <summary>
/// A utility class for fire.
/// </summary>
public static class FireHelper
{
	/// <summary>
	/// Starts a fire.
	/// </summary>
	/// <param name="origin">The starting point of the fire.</param>
	/// <param name="moveDirection">The direction the fire should be moving.</param>
	/// <param name="quantity">The amount of fire to spawn.</param>
	public static void StartFiresAt( Vector3 origin, Vector3 moveDirection, int quantity )
	{
		Host.AssertServer();

		for ( var i = 0; i < quantity; i++ )
			_ = new FireEntity( origin + Vector3.Random.WithY( 0 ) * 30, moveDirection + Vector3.Random.WithY( 0 ) * 30 );
	}
}
