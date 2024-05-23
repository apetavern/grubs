namespace Grubs.Helpers;

public partial class ExplosionHelper
{
	[ActionGraphNode( "grubs.explode" ), Title( "Explode" ), Group( "Grubs Actions" )]
	public static void GraphExplode( Component source, Vector3 position, float radius, float damage, float force )
	{
		Instance.Explode( source, position, radius, damage, force );
	}
}
