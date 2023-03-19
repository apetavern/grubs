namespace Grubs;

public static class EntityExtensions
{
	public static void SoundFromScreen( this Entity e, string sound )
	{
		Assert.True( Game.IsClient );

		if ( !e.IsValid() )
			return;

		var pos = e.Position.ToScreen();
		Sound.FromScreen( sound, pos.x, pos.y );
	}
}
