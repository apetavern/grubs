namespace Grubs.Terrain
{
    public interface IDestructableTerrain
	{
		public void ModifyCircle( Vector2 position, float radius, bool destroy );
		public void ModifyRectangle( Vector2 position, Vector2 size, bool destroy );
	}
}
