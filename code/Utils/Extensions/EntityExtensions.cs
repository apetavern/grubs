namespace Grubs;

public static class EntityExtensions
{
	public static Sound SoundFromScreen( this Entity e, string sound )
	{
		Assert.True( Game.IsClient );

		var pos = e.Position.ToScreen();
		return Sound.FromScreen( sound, pos.x, pos.y );
	}
}
