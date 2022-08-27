using Grubs.Player;

namespace Grubs.UI.World;

public class WormNametags
{
	private Dictionary<Entity, WormNametag> Nametags { get; } = new();

	public WormNametags()
	{
		Event.Register( this );
		Update();
	}

	private void Update()
	{
		foreach ( var worm in Entity.All.OfType<Worm>() )
		{
			if ( Nametags.ContainsKey( worm ) )
				continue;

			Nametags.Add( worm, new WormNametag( worm ) );
		}
	}

	[Event.Tick.Client]
	public void Frame()
	{
		Update();

		foreach ( var nametag in Nametags.Values )
			nametag.AddClass( $"team-{nametag.Worm.Team.TeamName}" );
	}
}
