namespace Grubs;

public static class EntityExtensions
{
	public static void SoundFromScreen( this Entity e, string sound )
	{
		Assert.True( Game.IsClient );

		if ( Game.LocalPawn is not Player player || !e.IsValid() )
			return;

		var pos = e.Position.ToScreen();
		var dist = player.GrubsCamera.Distance / 10f;
		pos = ((pos - 0.5f) * dist) + 0.5f;
		Sound.FromScreen( sound, -pos.x, -pos.y );
	}
}
